using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Repositories;

public interface IConversationMemberRepository
{
    Task<List<Member>> GetMembers(Guid conversationId);
    
    Task<List<MemberPermission>> GetPermissions(Guid conversationId);
    
    Task<bool> HasPermission(Guid conversationId, Guid userId, params ConversationPermissions[] permission);
    
    Task BlockMember(Guid conversationId, Guid conversationMemberId);

    Task AddMember(Guid requesterId, Guid conversationId, Guid memberId, ConversationPermissions permission);

    Task RemoveMember(Guid conversationId, Guid conversationMemberId);
    
    Task UpdateMemberPermission(Guid requesterId, Guid conversationId, Guid conversationMemberId, ConversationPermissions permission);
}