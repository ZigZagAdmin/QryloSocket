using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Services;

public interface IConversationsService
{
    Task<List<string>> GetConversations(Guid userId);
    
    Task<Guid> CreateConversation(Guid userId, string conversationName, Dictionary<Guid, ConversationPermissions> members, bool isPrivate);
    
    Task DeleteConversation(Guid userId, Guid conversationId);
  
    Task UpdateConversation(Guid userId, Guid conversationId, string conversationName);
}