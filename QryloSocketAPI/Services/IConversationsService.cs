using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Services;

public interface IConversationsService
{
    Task<List<string>> GetConversations(Guid userId);
    
    Task<Guid> CreateConversation(Guid userId, long userCreatedOn, string conversationName, Dictionary<Guid, ConversationPermissions> members, bool isPrivate);
    
    Task DeleteConversation(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn);
  
    Task UpdateConversation(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn,
        string conversationName);
    
    Task AddMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn, ConversationPermissions permission);
    
    Task RemoveMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn);
    
    Task BlockMember(Guid memberId, long createdOn, Guid requesterId, long requesterCreatedOn);
}