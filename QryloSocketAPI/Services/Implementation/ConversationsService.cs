using QryloSocketAPI.Models;
using QryloSocketAPI.Repositories;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Services.Implementation;

public class ConversationsService(IConversationRepository conversationRepository) : IConversationsService
{
    public async Task<List<string>> GetConversations(Guid userId)
    {
        return await conversationRepository.GetConversations(userId);
    }

    public async Task<Conversation> GetConversation(Guid conversationId, long createdOn)
    {
        return await conversationRepository.GetConversation(conversationId, createdOn);
    }

    public async Task<Guid> CreateConversation(Guid userId, long userCreatedOn, string conversationName, Dictionary<Guid, ConversationPermissions> members, bool isPrivate)
    {
        return await conversationRepository.Create(userId, userCreatedOn, conversationName, members, isPrivate);
    }

    public async Task DeleteConversation(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn)
    {
        await conversationRepository.Delete(conversationId, conversationCreatedOn, userId, userCreatedOn);
    }

    public async Task UpdateConversation(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn,
        string conversationName)
    {
        await conversationRepository.Update(conversationId, conversationCreatedOn, userId, userCreatedOn,
            conversationName);
    }

    public async Task AddMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn, ConversationPermissions permission)
    {
        await conversationRepository.AddMember(userId, userCreatedOn, conversationId, conversationCreatedOn, memberId,
            memberCreatedOn, permission);
    }

    public async Task RemoveMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn)
    {
        await conversationRepository.RemoveMember(userId, userCreatedOn, conversationId, conversationCreatedOn, memberId, memberCreatedOn);
    }

    public async Task BlockMember(Guid memberId, long createdOn, Guid requesterId, long requesterCreatedOn)
    {
        await conversationRepository.BlockMember(memberId, createdOn, requesterId, requesterCreatedOn);
    }
}