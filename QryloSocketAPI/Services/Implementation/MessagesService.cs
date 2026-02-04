using QryloSocketAPI.Models;
using QryloSocketAPI.Repositories;
using QryloSocketAPI.Utilities;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Services.Implementation;

public class MessagesService(IMessageRepository messageRepository, IConversationMemberRepository conversationMemberRepository) : IMessagesService
{
    public async Task SendMessage(Guid requesterId, Guid conversationId, List<MessagePart> parts, bool isAction)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,ConversationPermissions.Member))
        {
            throw new GlobalException("Access denied");
        }
        await messageRepository.Create(requesterId, conversationId, parts, isAction);
    }

    public async Task DeleteMessage(Guid requesterId, Guid conversationId, Guid messageId)
    {
        var isAdmin = await conversationMemberRepository.HasPermission(conversationId, requesterId,
            ConversationPermissions.Admin, ConversationPermissions.SuperAdmin);
     
        await messageRepository.Delete(messageId, conversationId, requesterId, isAdmin);
    }

    public async Task UpdateMessage(Guid requesterId, Guid conversationId, Guid messageId, List<MessagePart> parts)
    {
        await messageRepository.Update(messageId, conversationId, requesterId, parts);
    }

    public async Task ReadMessage(Guid memberMessageId, Guid memberId)
    {
        await messageRepository.Read(memberMessageId, memberId);
    }

    public async Task ReportMessage(Guid userId, Guid conversationId, Guid messageId, string comment)
    {
        await messageRepository.Report(messageId, conversationId, userId);
    }

    public async Task PinMessage(Guid requesterId, Guid conversationId, Guid messageId)
    {
        if (!await conversationMemberRepository.HasPermission(conversationId, requesterId,
                ConversationPermissions.Admin, ConversationPermissions.SuperAdmin))
        {
            throw new GlobalException("Access denied");
        }
        await messageRepository.Pin(messageId, conversationId, requesterId);
    }
}