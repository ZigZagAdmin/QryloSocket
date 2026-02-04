using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QryloSocketAPI.Database;
using QryloSocketAPI.Utilities;

namespace QryloSocketAPI.Repositories.Implementations;

public class MessageRepository(IDbContextFactory<QryloContext> contextFactory) : IMessageRepository
{
    public async Task Create(Guid conversationId, Guid userId, List<Models.MessagePart> parts, bool isAction)
    {
        var queryBuilder = new StringBuilder();
        var messageId = Guid.CreateVersion7();
        var createdOn = Utility.GetUnixTimestampMilliseconds(messageId);
        queryBuilder.AppendFormat($@"INSERT INTO ""Messages""
                                        (
                                             ""MessageId"",
                                             ""UserId"",
                                             ""UserCreatedOn"",
                                             ""ConversationId"",
                                             ""ConversationCreatedOn"",
                                             ""CreatedOn"",
                                             ""IsAction""
                                         )
                                        VALUES
                                        ( 
                                             {messageId},
                                             {userId},
                                             {Utility.GetUnixTimestampMilliseconds(userId)},
                                             {conversationId},
                                             {Utility.GetUnixTimestampMilliseconds(conversationId)},
                                             {createdOn},
                                             {(isAction ? 1 : 0)}
                                         );");
        foreach (var part in parts)
        {
            var partId = Guid.CreateVersion7();
            queryBuilder.AppendFormat(
                $@"INSERT INTO ""MessageParts"" 
                    (
                         ""MessagePartId"", 
                         ""MessageId"", 
                         ""MessageCreatedOn"", 
                         ""Type"", 
                         ""Order"", 
                         ""Content"", 
                         ""CreatedOn"",
                         ""ConversationId"", 
                         ""ConversationCreatedOn""
                    )
                   VALUES 
                    (
                        {partId}, 
                        {messageId}, 
                        {createdOn}, 
                        {part.Type}, 
                        {part.Order},
                        {part.Content}, 
                        {Utility.GetUnixTimestampMilliseconds(partId)},
                        {conversationId}, 
                        {Utility.GetUnixTimestampMilliseconds(conversationId)}
                    )");
        }
        queryBuilder.AppendFormat($@"INSERT INTO ""ConversationMemberMessages"" 
                                        (
                                             ""ConversationMemberMessageId"",
                                             ""ConversationMemberId"",
                                             ""ConversationMemberCreatedOn"",
                                             ""MessageId"",
                                             ""MessageCreatedOn"",
                                             ""CreatedOn""
                                         )
                                         SELECT 
                                             {Guid.CreateVersion7()}, 
                                             cm.""ConversationMemberId"", 
                                             cm.""CreatedOn"", 
                                             {messageId}, 
                                             {createdOn}, 
                                             {createdOn}
                                         FROM ""ConversationMembers"" cm 
                                         WHERE cm.""ConversationId"" = {conversationId} AND cm.""IsBlocked"" = 0 AND cm.""UserId"" <> {userId}");
        
        await using var context = await contextFactory.CreateDbContextAsync();
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                var query = FormattableStringFactory.Create(queryBuilder.ToString());
                var rowsAffected = await context.Database.ExecuteSqlInterpolatedAsync(query);
                if(rowsAffected == 0)
                    throw new GlobalException("Error saving message");
                    
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task Update(Guid messageId, Guid conversationId, Guid userId, List<Models.MessagePart> parts)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendFormat($"""
                    UPDATE "Messages"
                    SET "UpdatedOn" = {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}
                    WHERE "MessageId" = {messageId} AND "CreatedOn" = {Utility.GetUnixTimestampMilliseconds(messageId)}
                    AND "ConversationId" = {conversationId} AND "ConversationCreatedOn" = {Utility.GetUnixTimestampMilliseconds(conversationId)};

                    DELETE FROM "MessageParts" mp WHERE mp."MessageId" = {messageId};
                    """);
        foreach(var part in parts)
        {
            var partId = Guid.CreateVersion7();
            queryBuilder.AppendFormat(
                $"""
                 INSERT INTO "MessageParts" 
                     (
                          "MessagePartId", 
                          "MessageId", 
                          "MessageCreatedOn", 
                          "Type", 
                          "Order", 
                          "Content", 
                          "CreatedOn",
                          "ConversationId", 
                          "ConversationCreatedOn"
                     )
                    VALUES 
                     (
                         {partId}, 
                         {messageId}, 
                         {Utility.GetUnixTimestampMilliseconds(messageId)}, 
                         {part.Type}, 
                         {part.Order},
                         {part.Content}, 
                         {Utility.GetUnixTimestampMilliseconds(partId)},
                         {conversationId}, 
                         {Utility.GetUnixTimestampMilliseconds(conversationId)}
                     )
                 """);
        }
        await using var context = await contextFactory.CreateDbContextAsync();
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var query = FormattableStringFactory.Create(queryBuilder.ToString());
                var rowsAffected = await context.Database.ExecuteSqlInterpolatedAsync(query);
                if(rowsAffected == 0)
                    throw new GlobalException("Error updating message");
                    
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task Delete(Guid messageId, Guid conversationId, Guid userId, bool isAdmin)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            $"""
             
             CREATE TEMP TABLE tmp_messages_to_delete AS
             SELECT m."MessageId"
             FROM "Messages" m
             WHERE m."MessageId" = :messageId AND m."CreatedOn" = :createdOn AND (m."UserId" = :userId OR :isAdmin = true);
             
             DELETE FROM "Messages"
             WHERE "MessageId" IN (
                 SELECT "MessageId" FROM tmp_messages_to_delete
             );
             
             DELETE FROM "MessageParts"
             WHERE "MessageId" IN (
                 SELECT "MessageId" FROM tmp_messages_to_delete
             );
             
             DROP TABLE tmp_messages_to_delete
             
             """,
            new NpgsqlParameter("messageId", messageId),
            new NpgsqlParameter("createdOn", Utility.GetUnixTimestampMilliseconds(messageId)),
            new NpgsqlParameter("userId", userId),
            new NpgsqlParameter("isAdmin", isAdmin)
        );
    }

    public async Task Read(Guid memberMessageId, Guid memberId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            UPDATE "ConversationMemberMessages" 
            SET "IsRead" = 1,
                "ReadOn" = :now
            WHERE "ConversationMemberMessageId" = :memberMessageId AND "CreatedOn" = :createdOn 
            AND "ConversationMemberId" = :memberId AND "ConversationMemberCreatedOn" = :memberCreatedOn AND "IsRead" = 0
            """,
            new NpgsqlParameter("memberMessageId", memberMessageId),
            new NpgsqlParameter("createdOn", Utility.GetUnixTimestampMilliseconds(memberMessageId)),
            new NpgsqlParameter("memberId", memberId),
            new NpgsqlParameter("memberCreatedOn", Utility.GetUnixTimestampMilliseconds(memberId)),
            new NpgsqlParameter("now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        );
    }

    public async Task Pin(Guid messageId, Guid conversationId, Guid userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            UPDATE "Messages"
            SET "IsPinned" = 1
            WHERE "MessageId" = :messageId AND "CreatedOn" = :createdOn
            AND "ConversationId" = :conversationId AND "ConversationCreatedOn" = :conversationCreatedOn
            """,
            new NpgsqlParameter("messageId", messageId),
            new NpgsqlParameter("createdOn", Utility.GetUnixTimestampMilliseconds(messageId)),
            new NpgsqlParameter("conversationId", conversationId),
            new NpgsqlParameter("conversationCreatedOn", Utility.GetUnixTimestampMilliseconds(conversationId))
        );
    }

    public async Task Report(Guid messageId, Guid conversationId, Guid userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
    }
}