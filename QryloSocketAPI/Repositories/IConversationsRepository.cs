using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities.Enums;
using File = QryloSocketAPI.Models.File;

namespace QryloSocketAPI.Repositories;

public interface IConversationsRepository
{
    Task<List<string>> GetConversations(Guid userId);
    
    Task<Conversation> GetConversation(Guid conversationId, long createdOn);
    
    Task<Guid> Create(Guid userId, long userCreatedOn, string name, Dictionary<Guid, ConversationPermissions> users,
        bool isPrivate, File? avatar = null);

    Task Update(Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn, string name);

    Task Delete(Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn);
    
    Task BlockMember(Guid memberId, long createdOn, Guid requesterId, long requesterCreatedOn);

    Task AddMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn, ConversationPermissions permission);

    Task RemoveMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn);
}