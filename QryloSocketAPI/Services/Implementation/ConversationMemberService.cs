using QryloSocketAPI.Models;
using QryloSocketAPI.Repositories;
using QryloSocketAPI.Utilities;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Services.Implementation;

public class ConversationMemberService(IConversationMemberRepository conversationMemberRepository) : IConversationMemberService
{
    public async Task<List<Member>> GetMembers(Guid requesterId, Guid conversationId)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                ConversationPermissions.Member))
        {
            throw new GlobalException("Access denied");
        }
        return await conversationMemberRepository.GetMembers(conversationId);
    }

    public async Task<List<MemberPermission>> GetPermissions(Guid requesterId, Guid conversationId)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                ConversationPermissions.Member))
        {
            throw new GlobalException("Access denied");
        }
        return await conversationMemberRepository.GetPermissions(conversationId);
    }

    public async Task AddMember(Guid requesterId, Guid conversationId, Guid memberId, ConversationPermissions permission)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                ConversationPermissions.Admin, ConversationPermissions.SuperAdmin))
        {
            throw new GlobalException("Access denied");
        }
        await conversationMemberRepository.AddMember(requesterId, conversationId, memberId, permission);
    }

    public async Task RemoveMember(Guid requesterId, Guid conversationId, Guid conversationMemberId)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                ConversationPermissions.Admin, ConversationPermissions.SuperAdmin))
        {
            throw new GlobalException("Access denied");
        }
        await conversationMemberRepository.RemoveMember(conversationId, conversationMemberId);
    }

    public async Task BlockMember(Guid conversationId, Guid conversationMemberId, Guid requesterId)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                ConversationPermissions.Admin, ConversationPermissions.SuperAdmin))
        {
            throw new GlobalException("Access denied");
        }
        await conversationMemberRepository.BlockMember(conversationId, conversationMemberId);
    }

    public async Task UpdateMemberPermission(Guid requesterId, Guid conversationId, Guid conversationMemberId, ConversationPermissions permission)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                 ConversationPermissions.SuperAdmin))
        {
            throw new GlobalException("Access denied");
        }
        await conversationMemberRepository.UpdateMemberPermission(requesterId, conversationId, conversationMemberId, permission);
    }
}