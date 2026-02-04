using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities.Enums;
using File = QryloSocketAPI.Models.File;

namespace QryloSocketAPI.Repositories;

public interface IConversationRepository
{
    Task<List<string>> GetConversations(Guid userId);
    
    Task<Conversation> GetConversation(Guid conversationId);
    
    Task<Guid> Create(Guid userId, string name, Dictionary<Guid, ConversationPermissions> users,
        bool isPrivate, File? avatar = null);

    Task Update(Guid conversationId, string name, string avatar);

    Task Delete(Guid conversationId);
}