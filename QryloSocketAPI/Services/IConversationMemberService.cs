using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Services;

public interface IConversationMemberService
{
    Task<List<Member>> GetMembers(Guid requesterId, Guid conversationId);
    
    Task<List<MemberPermission>> GetPermissions(Guid requesterId, Guid conversationId);
    
    Task AddMember(Guid requesterId, Guid conversationId, Guid memberId, ConversationPermissions permission);
    
    Task RemoveMember(Guid requesterId, Guid conversationId, Guid conversationMemberId);
    
    Task BlockMember(Guid conversationId, Guid conversationMemberId, Guid requesterId);
    
    Task UpdateMemberPermission(Guid requesterId, Guid conversationId, Guid conversationMemberId, ConversationPermissions permission);
}