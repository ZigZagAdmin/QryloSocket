using QryloSocketAPI.Repositories;

namespace QryloSocketAPI.Services.Implementation;

public class MessagesService(IMessageRepository messageRepository) : IMessagesService
{
    public async Task SendMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, string message, bool isAction)
    {
        await messageRepository.Create(userId, userCreatedOn, conversationId, conversationCreatedOn, message, isAction);
    }

    public async Task DeleteMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId,
        long messageCreatedOn)
    {
        await messageRepository.Delete(messageId, messageCreatedOn, conversationId,conversationCreatedOn, userId, userCreatedOn);
    }

    public async Task UpdateMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId,
        long messageCreatedOn, string message)
    {
        await messageRepository.Update(messageId, messageCreatedOn, conversationId, conversationCreatedOn, userId, userCreatedOn, message);
    }

    public async Task ReadMessage(Guid memberMessageId, long messageCreatedOn, Guid memberId, long memberCreatedOn)
    {
        await messageRepository.Read(memberMessageId, messageCreatedOn, memberId, memberCreatedOn);
    }

    public async Task ReportMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId,
        long messageCreatedOn, string comment)
    {
        await messageRepository.Report(messageId, messageCreatedOn, conversationId, conversationCreatedOn, userId,
            userCreatedOn);
    }

    public async Task PinMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId,
        long messageCreatedOn)
    {
        await messageRepository.Pin(messageId, messageCreatedOn, conversationId, conversationCreatedOn, userId,
            userCreatedOn);
    }

    public async Task MarkAsDelivered(Guid memberMessageId, long messageCreatedOn, Guid memberId, long memberCreatedOn)
    {
        await messageRepository.MarkAsDelivered(memberMessageId, messageCreatedOn, memberId, memberCreatedOn);
    }
}