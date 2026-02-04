using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using QryloSocketAPI.Database;
using QryloSocketAPI.Models;
using QryloSocketAPI.Records;
using QryloSocketAPI.Services;
using QryloSocketAPI.Utilities;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Repositories.Implementations;

public class ConversationMemberRepository(
    IDbContextFactory<QryloContext> contextFactory,
    ICachingService<List<Member>> cachingService) : IConversationMemberRepository
{
    public async Task<List<Member>> GetMembers(Guid conversationId)
    {
        var cacheKey = $"ConversationMembers_{conversationId}";
        var cacheDuration = TimeSpan.FromMinutes(5);
        return await cachingService.GetOrAddAsync(cacheKey, async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var query = @"SELECT 
                               cm.""ConversationMemberId"",
                               cm.""UserId"" AS ""MemberId"",
                               up.""Name"",
                               cm.""Permission"",
                               cm.""IsBlocked"" = 1 AS ""IsBlocked""
                        FROM ""ConversationMembers"" cm
                                 INNER JOIN ""UnifiedProfiles"" up ON up.""UserId"" = cm.""UserId"" AND up.""CreatedOn"" = cm.""UserCreatedOn""
                        WHERE cm.""ConversationId"" = :conversationId";
            var parameters = new object[]
            {
                new NpgsqlParameter("conversationId", NpgsqlDbType.Uuid) { Value = conversationId }
            };
            return context.Set<MembersRecord>()
                .FromSqlRaw(query, parameters)
                .AsEnumerable()
                .Select(m => new Member(m.ConversationMemberId, m.MemberId, m.Name, Enum
                    .GetValues<ConversationPermissions>()
                    .Where(p => p != ConversationPermissions.None &&
                                (m.Permission & p) == p)
                    .ToArray(), m.IsBlocked))
                .ToList();
        }, cacheDuration);
    }

    public async Task<List<MemberPermission>> GetPermissions(Guid conversationId)
    {
        var keys = await cachingService.GetByPatternAsync(Utility.GetMemberPermissionKey(conversationId.ToString()));
        if (keys?.Count > 0)
        {
            return keys.Select(key => new MemberPermission(Guid.Parse(key.Split(':')[2]), Enum
                .GetValues<ConversationPermissions>()
                .Where(p => p != ConversationPermissions.None &&
                            ((ConversationPermissions)Convert.ToInt32(key.Split(':')[3]) & p) == p)
                .ToArray())
            ).ToList();
        }
        await using var context = await contextFactory.CreateDbContextAsync();
        var permissions = await context.ConversationMembers
            .AsNoTracking()
            .Where(p => p.ConversationId == conversationId)
            .ToListAsync();
        
        await cachingService.SetKeys(permissions.Select(perm =>
            Utility.GetMemberPermissionKey(perm.ConversationId.ToString(), perm.UserId.ToString(), perm.Permission.ToString())).ToHashSet());
        
        return permissions.Select(p => new MemberPermission(p.UserId, Enum
            .GetValues<ConversationPermissions>()
            .Where(pp => pp != ConversationPermissions.None &&
                         (p.Permission & pp) == pp)
            .ToArray())).ToList();
    }

    public async Task<bool> HasPermission(Guid conversationId, Guid userId, params ConversationPermissions[] permission)
    {
        var permissions = await GetPermissions(conversationId);
        var memberPermissions = permissions.FirstOrDefault(x => x.MemberId == userId);
        if(memberPermissions == null) return false;
        
        var userPerms = memberPermissions.Permissions.Aggregate(
            ConversationPermissions.None,
            (current, perm) => current | perm);

        var requiredPerms = permission.Aggregate(
            ConversationPermissions.None,
            (current, perm) => current | perm);

        return (userPerms & requiredPerms) != 0;
    }

    public async Task BlockMember(Guid conversationId, Guid conversationMemberId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = @"UPDATE ""ConversationMembers"" 
                        SET ""IsBlocked"" = 1
                        WHERE ""ConversationMemberId"" = :conversationMemberId AND ""CreatedOn"" = :createdOn";
        var parameters = new List<NpgsqlParameter>
        {
            new("memberId", NpgsqlDbType.Uuid) { Value = conversationMemberId },
            new("createdOn", NpgsqlDbType.Bigint) { Value = Utility.GetUnixTimestampMilliseconds(conversationMemberId) }
        };
        await cachingService.InvalidateCacheAsync($"ConversationMembers_{conversationId}");
        await context.Database.ExecuteSqlRawAsync(query, parameters);
        await cachingService.DeleteByPattern(Utility.GetMemberPermissionKey(conversationId.ToString()));
    }

    public async Task AddMember(Guid userId, Guid conversationId, Guid memberId, ConversationPermissions permission)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var conversationMemberId = Guid.CreateVersion7();
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
            new NpgsqlParameter("conversationMemberId", conversationMemberId),
            new NpgsqlParameter("conversationId", conversationId),
            new NpgsqlParameter("createdOn", Utility.GetUnixTimestampMilliseconds(conversationId)),
            new NpgsqlParameter("memberId", memberId),
            new NpgsqlParameter("memberCreatedOn", Utility.GetUnixTimestampMilliseconds(memberId)),
            new NpgsqlParameter("permission", permission),
            new NpgsqlParameter("now", Utility.GetUnixTimestampMilliseconds(conversationMemberId)),
            new NpgsqlParameter("createdBy", userId),
            new NpgsqlParameter("createdByCreatedOn", Utility.GetUnixTimestampMilliseconds(userId))
        );
        await cachingService.InvalidateCacheAsync($"ConversationMembers_{conversationId}");
        await cachingService.DeleteByPattern(Utility.GetMemberPermissionKey(conversationId.ToString()));
    }

    public async Task RemoveMember(Guid conversationId, Guid conversationMemberId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM "ConversationMembers"
            WHERE "ConversationMemberId" = :conversationMemberId AND "CreatedOn" = :createdOn
            """,
            new NpgsqlParameter("conversationMemberId", conversationMemberId),
            new NpgsqlParameter("createdOn", Utility.GetUnixTimestampMilliseconds(conversationId))
        );
        await cachingService.InvalidateCacheAsync($"ConversationMembers_{conversationId}");
        await cachingService.DeleteByPattern(Utility.GetMemberPermissionKey(conversationId.ToString()));
    }

    public async Task UpdateMemberPermission(Guid userId, Guid conversationId, Guid conversationMemberId,
        ConversationPermissions permission)
    {
        await cachingService.InvalidateCacheAsync($"ConversationMembers_{conversationId}");
        await cachingService.DeleteByPattern(Utility.GetMemberPermissionKey(conversationId.ToString()));
    }
}