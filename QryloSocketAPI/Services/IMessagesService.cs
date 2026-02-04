using QryloSocketAPI.Models;

namespace QryloSocketAPI.Services;

public interface IMessagesService
{
    Task SendMessage(Guid requesterId, Guid conversationId, List<MessagePart> parts, bool isAction);

    Task DeleteMessage(Guid requesterId, Guid conversationId, Guid messageId);

    Task UpdateMessage(Guid requesterId, Guid conversationId, Guid messageId, List<MessagePart> parts);

    Task ReadMessage(Guid memberMessageId, Guid memberId);

    Task ReportMessage(Guid requesterId, Guid conversationId, Guid messageId, string comment);

    Task PinMessage(Guid requesterId, Guid conversationId, Guid messageId);
}