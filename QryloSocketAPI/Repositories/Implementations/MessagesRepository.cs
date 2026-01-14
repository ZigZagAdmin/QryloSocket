using Microsoft.EntityFrameworkCore;
using Npgsql;
using QryloSocketAPI.Database;

namespace QryloSocketAPI.Repositories.Implementations;

public class MessagesRepository(IDbContextFactory<QryloContext> contextFactory) : IMessagesRepository
{
    public async Task Create(Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn,
        string text, bool isAction)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "Messages"
            ("MessageId",
             "UserId",
             "UserCreatedOn",
             "ConversationId",
             "ConversationCreatedOn",
             "Text",
             "CreatedOn",
             "IsAction")
            VALUES
            (:messageId,
             :userId,
             :userCreatedOn,
             :conversationId,
             :conversationCreatedOn,
             :text,
             :createdOn,
             :isAction);
            INSERT INTO "ConversationMemberMessages" 
            (
             "ConversationMemberMessageId",
             "ConversationMemberId",
             "ConversationMemberCreatedOn",
             "MessageId",
             "MessageCreatedOn",
             "CreatedOn"
             )
             SELECT 
                 :memberMessageId, 
                 cm."ConversationMemberId", 
                 cm."CreatedOn", 
                 :messageId, 
                 :createdOn, 
                 :createdOn
             FROM "ConversationMembers" cm 
             WHERE cm."ConversationId" = :conversationId AND cm."IsBlocked" = 0
            """,
            new NpgsqlParameter("messageId", Guid.CreateVersion7()),
            new NpgsqlParameter("userId", userId),
            new NpgsqlParameter("userCreatedOn", userCreatedOn),
            new NpgsqlParameter("conversationId", conversationId),
            new NpgsqlParameter("conversationCreatedOn", conversationCreatedOn),
            new NpgsqlParameter("text", text),
            new NpgsqlParameter("createdOn", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            new NpgsqlParameter("isAction", isAction),
            new NpgsqlParameter("memberMessageId", Guid.CreateVersion7())
        );
    }

    public async Task Update(Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn,
        Guid userId,
        long userCreatedOn, string text)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            UPDATE "Messages"
            SET "Text" = :text
            WHERE "MessageId" = :messageId AND "CreatedOn" = :createdOn
            AND "ConversationId" = :conversationId AND "ConversationCreatedOn" = :conversationCreatedOn
            """,
            new NpgsqlParameter("messageId", messageId),
            new NpgsqlParameter("createdOn", messageCreatedOn),
            new NpgsqlParameter("conversationId", conversationId),
            new NpgsqlParameter("conversationCreatedOn", conversationCreatedOn),
            new NpgsqlParameter("text", text)
        );
    }

    public async Task Delete(Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn,
        Guid userId,
        long userCreatedOn)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM "Messages"
            WHERE "MessageId" = :messageId AND "CreatedOn" = :createdOn
            AND "ConversationId" = :conversationId AND "ConversationCreatedOn" = :conversationCreatedOn
            """,
            new NpgsqlParameter("messageId", messageId),
            new NpgsqlParameter("createdOn", messageCreatedOn),
            new NpgsqlParameter("conversationId", conversationId),
            new NpgsqlParameter("conversationCreatedOn", conversationCreatedOn)
        );
    }

    public async Task Read(Guid memberMessageId, long messageCreatedOn,  Guid memberId,  long memberCreatedOn)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            UPDATE "ConversationMemberMessages" 
            SET "IsRead" = 1
            WHERE "ConversationMemberMessageId" = :memberMessageId AND "CreatedOn" = :createdOn 
            AND "ConversationMemberId" = :memberId AND "ConversationMemberCreatedOn" = :memberCreatedOn
            """,
            new NpgsqlParameter("memberMessageId", memberMessageId),
            new NpgsqlParameter("createdOn", messageCreatedOn),
            new NpgsqlParameter("memberId", memberId),
            new NpgsqlParameter("memberCreatedOn", memberCreatedOn)
        );
    }

    public async Task MarkAsDelivered(Guid memberMessageId, long messageCreatedOn,  Guid memberId,  long memberCreatedOn)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            UPDATE "ConversationMemberMessages" 
            SET "IsDelivered" = 1
            WHERE "ConversationMemberMessageId" = :memberMessageId AND "CreatedOn" = :createdOn 
            AND "ConversationMemberId" = :memberId AND "ConversationMemberCreatedOn" = :memberCreatedOn
            """,
            new NpgsqlParameter("memberMessageId", memberMessageId),
            new NpgsqlParameter("createdOn", messageCreatedOn),
            new NpgsqlParameter("memberId", memberId),
            new NpgsqlParameter("memberCreatedOn", memberCreatedOn)
        );
    }

    public async Task Pin(Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn,
        Guid userId,
        long userCreatedOn)
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
            new NpgsqlParameter("createdOn", messageCreatedOn),
            new NpgsqlParameter("conversationId", conversationId),
            new NpgsqlParameter("conversationCreatedOn", conversationCreatedOn)
        );
    }

    public async Task Report(Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn,
        Guid userId,
        long userCreatedOn)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
    }
}