using QryloSocketAPI.Models;
using QryloSocketAPI.Repositories;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Services.Implementation;

public class ConversationsService(IConversationsRepository conversationsRepository) : IConversationsService
{
    public async Task<List<string>> GetConversations(Guid userId)
    {
        return await conversationsRepository.GetConversations(userId);
    }

    public async Task<Conversation> GetConversation(Guid conversationId, long createdOn)
    {
        return await conversationsRepository.GetConversation(conversationId, createdOn);
    }

    public async Task<Guid> CreateConversation(Guid userId, long userCreatedOn, string conversationName, Dictionary<Guid, ConversationPermissions> members, bool isPrivate)
    {
        return await conversationsRepository.Create(userId, userCreatedOn, conversationName, members, isPrivate);
    }

    public async Task DeleteConversation(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn)
    {
        await conversationsRepository.Delete(conversationId, conversationCreatedOn, userId, userCreatedOn);
    }

    public async Task UpdateConversation(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn,
        string conversationName)
    {
        await conversationsRepository.Update(conversationId, conversationCreatedOn, userId, userCreatedOn,
            conversationName);
    }

    public async Task AddMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn, ConversationPermissions permission)
    {
        await conversationsRepository.AddMember(userId, userCreatedOn, conversationId, conversationCreatedOn, memberId,
            memberCreatedOn, permission);
    }

    public async Task RemoveMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId,
        long memberCreatedOn)
    {
        await conversationsRepository.RemoveMember(userId, userCreatedOn, conversationId, conversationCreatedOn, memberId, memberCreatedOn);
    }

    public async Task BlockMember(Guid memberId, long createdOn, Guid requesterId, long requesterCreatedOn)
    {
        await conversationsRepository.BlockMember(memberId, createdOn, requesterId, requesterCreatedOn);
    }
}