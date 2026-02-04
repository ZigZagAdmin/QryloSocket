using QryloSocketAPI.Models;
using QryloSocketAPI.Repositories;
using QryloSocketAPI.Utilities;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Services.Implementation;

public class ConversationsService(IConversationRepository conversationRepository, IConversationMemberRepository conversationMemberRepository) : IConversationsService
{
    public async Task<List<string>> GetConversations(Guid userId)
    {
        return await conversationRepository.GetConversations(userId);
    }

    public async Task<Conversation> GetConversation(Guid requesterId, Guid conversationId)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                ConversationPermissions.Member))
        {
            throw new GlobalException("Access denied");
        }
        return await conversationRepository.GetConversation(conversationId);
    }

    public async Task<Guid> CreateConversation(Guid requesterId, string conversationName, Dictionary<Guid, ConversationPermissions> members, bool isPrivate)
    {
        return await conversationRepository.Create(requesterId, conversationName, members, isPrivate);
    }

    public async Task DeleteConversation(Guid requesterId, Guid conversationId)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,ConversationPermissions.SuperAdmin))
        {
            throw new GlobalException("Access denied");
        }
        await conversationRepository.Delete(conversationId);
    }

    public async Task UpdateConversation(Guid requesterId, Guid conversationId, string conversationName)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                ConversationPermissions.Admin, ConversationPermissions.SuperAdmin))
        {
            throw new GlobalException("Access denied");
        }
        await conversationRepository.Update(conversationId, conversationName, string.Empty);
    }
}