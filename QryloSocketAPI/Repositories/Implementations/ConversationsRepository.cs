using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using QryloSocketAPI.Database;
using QryloSocketAPI.Models;
using QryloSocketAPI.Services;
using QryloSocketAPI.Utilities.Enums;
using File = QryloSocketAPI.Models.File;

namespace QryloSocketAPI.Repositories.Implementations;

public class ConversationsRepository(IDbContextFactory<QryloContext> contextFactory, ICachingService<List<string>> cachingListService, ICachingService<Conversation> cachingService) : IConversationsRepository
{
    public async Task<List<string>> GetConversations(Guid userId)
    {
        var cacheKey = $"UserConversations_{userId}";
        var cacheDuration = TimeSpan.FromMinutes(5);
        return await cachingListService.GetOrAddAsync(cacheKey, async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.ConversationMembers
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.IsBlocked == 0)
                .Select(c => c.ConversationId.ToString())
                .ToListAsync();
        }, cacheDuration);
    }

    public async Task<Conversation> GetConversation(Guid conversationId, long createdOn)
    {
        var cacheKey = $"Conversation_{conversationId}";
        var cacheDuration = TimeSpan.FromMinutes(5);
        return await cachingService.GetOrAddAsync(cacheKey, async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Conversations
                .AsNoTracking()
                .Where(c => c.ConversationId == conversationId && c.CreatedOn == createdOn)
                .Select(c => new Conversation(c.ConversationId, c.Name, c.CreatedOn, c.IsPrivate == 1))
                .FirstOrDefaultAsync();
        }, cacheDuration);
    }

    public async Task<Guid> Create(Guid userId, long userCreatedOn, string name, Dictionary<Guid, ConversationPermissions> users, bool isPrivate, File? avatar = null)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var conversationId = Guid.CreateVersion7();
        var parameters = new List<NpgsqlParameter>
        {
            new("conversationId", NpgsqlDbType.Uuid) { Value = conversationId },
            new("name", NpgsqlDbType.Varchar) { Value = name },
            new("avatar", NpgsqlDbType.Varchar) { Value = avatar },
            new("createdOn", NpgsqlDbType.Bigint) { Value = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
            new("isPrivate", NpgsqlDbType.Smallint) { Value = (short)(isPrivate ? 1 : 0) },
            new("userId", NpgsqlDbType.Uuid) { Value = userId },
            new("userCreatedOn", NpgsqlDbType.Bigint) { Value = userCreatedOn }
        };
        var queryBuilder = new StringBuilder();
        queryBuilder.Append(
            @"INSERT INTO ""Conversations"" (""ConversationId"", ""Name"", ""Avatar"", ""CreatedOn"", ""IsPrivate"") 
                VALUES(:conversationId, :name, :avatar, :createdOn, :isPrivate); ");
        var index = 1;
        foreach (var user in users)
        {
            queryBuilder.Append(
                $@"INSERT INTO ""ConversationMembers"" (""ConversationMemberId"", ""ConversationId"", ""ConversationCreatedOn"", ""UserId"", ""UserCreatedOn"", ""Permission"", ""IsBlocked"", ""CreatedOn"", ""CreatedBy"", ""CreatedByCreatedOn"") 
                SELECT :memberId, :conversationId, :createdOn, u.""UserId"", u.""CreatedOn"", :permission_{index}, 0, :createdOn, :userId, :userCreatedOn
                FROM ""Users"" u WHERE u.""Status"" = 1 AND u.""UserId"" = :userId_{index}; ");
            parameters.Add(new NpgsqlParameter($"memberId_{index}", NpgsqlDbType.Uuid) { Value = Guid.CreateVersion7() });
            parameters.Add(new NpgsqlParameter($"userId_{index}", NpgsqlDbType.Uuid) { Value = user.Key });
            parameters.Add(new NpgsqlParameter($"permission_{index}", NpgsqlDbType.Integer) { Value = user.Value });
            index++;
        }
        await context.Database.ExecuteSqlRawAsync(queryBuilder.ToString(), parameters);
        return conversationId;
    }

    public async Task Update(Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn, string name)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var parameters = new List<NpgsqlParameter>
        {
            new("conversationId", NpgsqlDbType.Uuid) { Value = conversationId },
            new("name", NpgsqlDbType.Varchar) { Value = name },
            new("createdOn", NpgsqlDbType.Bigint) { Value = conversationCreatedOn },
            new("userId", NpgsqlDbType.Uuid) { Value = userId },
            new("userCreatedOn", NpgsqlDbType.Bigint) { Value = userCreatedOn }
        };
        
        var query =
            @"UPDATE ""Conversations""
                SET
                    ""Name"" = :name,
                    ""Avatar"" = :avatar
                WHERE ""ConversationId"" = :conversationId AND ""CreatedOn"" = :createdOn;

            DELETE FROM ""ConversationMembers"" WHERE ""ConversationId"" = :conversationId";
       
        await context.Database.ExecuteSqlRawAsync(query, parameters);
    }

    public async Task Delete(Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = @"DELETE FROM ""Conversations"" WHERE ""ConversationId"" = :conversationId AND ""CreatedOn"" = :createdOn;
                      DELETE FROM ""ConversationMembers"" WHERE ""ConversationId"" = :conversationId;
                      DELETE FROM ""Messages"" WHERE ""ConversationId"" = :conversationId;";
        var parameters = new List<NpgsqlParameter>
        {
            new("conversationId", NpgsqlDbType.Uuid) { Value = conversationId },
            new("createdOn", NpgsqlDbType.Bigint) { Value = conversationCreatedOn }
        };
        await context.Database.ExecuteSqlRawAsync(query, parameters);
    }  

    public async Task BlockMember(Guid memberId, long createdOn, Guid requesterId, long requesterCreatedOn)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = @"UPDATE ""ConversationMembers"" 
                        SET ""IsBlocked"" = 1
                        WHERE ""ConversationMemberId"" = :memberId AND ""CreatedOn"" = :createdOn";
        var parameters = new List<NpgsqlParameter>
        {
            new("memberId", NpgsqlDbType.Uuid) { Value = memberId },
            new("createdOn", NpgsqlDbType.Bigint) { Value = createdOn }
        };
        await context.Database.ExecuteSqlRawAsync(query, parameters);
    }

    public async Task AddMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn, ConversationPermissions permission)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "ConversationMembers"
            ("ConversationMemberId",
             "ConversationId",
             "ConversationCreatedOn",
             "UserId",
             "UserCreatedOn",
             "Permission",
             "IsBlocked",
             "CreatedOn",
             "CreatedBy",
             "CreatedByCreatedOn")
            VALUES
            (:conversationMemberId,
             :conversationId,
             :createdOn,
             :memberId,
             :memberCreatedOn,
             :permission,
             0,
             :now,
             :createdBy,
             :createdByCreatedOn)
            """,
            new NpgsqlParameter("conversationMemberId", Guid.CreateVersion7()),
            new NpgsqlParameter("conversationId", conversationId),
            new NpgsqlParameter("createdOn", conversationCreatedOn),
            new NpgsqlParameter("memberId", memberId),
            new NpgsqlParameter("memberCreatedOn", memberCreatedOn),
            new NpgsqlParameter("permission", permission),
            new NpgsqlParameter("now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            new NpgsqlParameter("createdBy", userId),
            new NpgsqlParameter("createdByCreatedOn", userCreatedOn)
        );
    }

    public async Task RemoveMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM "ConversationMembers"
            WHERE "ConversationId" = :conversationId AND "CreatedOn" = :conversationCreatedOn AND "UserId" = :memberId AND "UserCreatedOn" = :memberCreatedOn
            """,
            new NpgsqlParameter("conversationId", conversationId),
            new NpgsqlParameter("createdOn", conversationCreatedOn),
            new NpgsqlParameter("memberId", memberId),
            new NpgsqlParameter("memberCreatedOn", memberCreatedOn)
        );
    }
}