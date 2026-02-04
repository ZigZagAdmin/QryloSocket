using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using QryloSocketAPI.Database;
using QryloSocketAPI.Models;
using QryloSocketAPI.Services;
using QryloSocketAPI.Utilities;
using QryloSocketAPI.Utilities.Enums;
using File = QryloSocketAPI.Models.File;

namespace QryloSocketAPI.Repositories.Implementations;

public class ConversationRepository(
    IDbContextFactory<QryloContext> contextFactory,
    ICachingService<List<string>> cachingListService,
    ICachingService<Conversation> cachingService) : IConversationRepository
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

    public async Task<Conversation> GetConversation(Guid conversationId)
    {
        var cacheKey = $"Conversation_{conversationId}";
        var cacheDuration = TimeSpan.FromMinutes(5);
        return await cachingService.GetOrAddAsync(cacheKey, async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Conversations
                .AsNoTracking()
                .Where(c => c.ConversationId == conversationId &&
                            c.CreatedOn == Utility.GetUnixTimestampMilliseconds(conversationId))
                .Select(c => new Conversation(c.ConversationId, c.Name, c.CreatedOn, c.IsPrivate == 1))
                .FirstOrDefaultAsync() ?? throw new GlobalException("Conversation not found");
        }, cacheDuration);
    }

    public async Task<Guid> Create(Guid userId, string name, Dictionary<Guid, ConversationPermissions> users,
        bool isPrivate, File? avatar = null)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var conversationId = Guid.CreateVersion7();
        var parameters = new List<NpgsqlParameter>
        {
            new("conversationId", NpgsqlDbType.Uuid) { Value = conversationId },
            new("createdOn", NpgsqlDbType.Uuid) { Value = Utility.GetUnixTimestampMilliseconds(conversationId) },
            new("name", NpgsqlDbType.Varchar) { Value = name },
            new("avatar", NpgsqlDbType.Varchar) { Value = avatar },
            new("isPrivate", NpgsqlDbType.Smallint) { Value = (short)(isPrivate ? 1 : 0) },
            new("userId", NpgsqlDbType.Uuid) { Value = userId },
            new("userCreatedOn", NpgsqlDbType.Bigint) { Value = Utility.GetUnixTimestampMilliseconds(userId) }
        };
        var queryBuilder = new StringBuilder();
        queryBuilder.Append(
            @"INSERT INTO ""Conversations"" (""ConversationId"", ""Name"", ""Avatar"", ""CreatedOn"", ""IsPrivate"") 
                VALUES(:conversationId, :name, :avatar, :createdOn, :isPrivate); ");
        var index = 1;
        foreach (var user in users)
        {
            var memberId = Guid.CreateVersion7();
            queryBuilder.Append(
                $@"INSERT INTO ""ConversationMembers"" (""ConversationMemberId"", ""ConversationId"", ""ConversationCreatedOn"", ""UserId"", ""UserCreatedOn"", ""Permission"", ""IsBlocked"", ""CreatedOn"", ""CreatedBy"", ""CreatedByCreatedOn"") 
                SELECT :memberId, :conversationId, :createdOn, u.""UserId"", u.""CreatedOn"", :permission_{index}, 0, :createdOn_{index}, :userId, :userCreatedOn
                FROM ""Users"" u WHERE u.""Status"" = 1 AND u.""UserId"" = :userId_{index}; ");
            parameters.Add(new NpgsqlParameter($"memberId_{index}", NpgsqlDbType.Uuid) { Value = memberId });
            parameters.Add(new NpgsqlParameter($"createdOn_{index}", NpgsqlDbType.Bigint)
                { Value = Utility.GetUnixTimestampMilliseconds(memberId) });
            parameters.Add(new NpgsqlParameter($"userId_{index}", NpgsqlDbType.Uuid) { Value = user.Key });
            parameters.Add(new NpgsqlParameter($"permission_{index}", NpgsqlDbType.Integer) { Value = user.Value });
            index++;
        }

        await context.Database.ExecuteSqlRawAsync(queryBuilder.ToString(), parameters);
        return conversationId;
    }

    public async Task Update(Guid conversationId, string name, string avatar)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var parameters = new List<NpgsqlParameter>
        {
            new("conversationId", NpgsqlDbType.Uuid) { Value = conversationId },
            new("name", NpgsqlDbType.Varchar) { Value = name },
            new("avatar", NpgsqlDbType.Varchar) { Value = avatar },
            new("createdOn", NpgsqlDbType.Bigint) { Value = Utility.GetUnixTimestampMilliseconds(conversationId) }
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

    public async Task Delete(Guid conversationId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query =
            @"DELETE FROM ""Conversations"" WHERE ""ConversationId"" = :conversationId AND ""CreatedOn"" = :createdOn;
              DELETE FROM ""ConversationMembers"" WHERE ""ConversationId"" = :conversationId;
              DELETE FROM ""Messages"" WHERE ""ConversationId"" = :conversationId;
              DELETE FROM ""MessageParts"" WHERE ""ConversationId"" = :conversationId;";
        var parameters = new List<NpgsqlParameter>
        {
            new("conversationId", NpgsqlDbType.Uuid) { Value = conversationId },
            new("createdOn", NpgsqlDbType.Bigint) { Value = Utility.GetUnixTimestampMilliseconds(conversationId) }
        };
        await context.Database.ExecuteSqlRawAsync(query, parameters);
    }
}