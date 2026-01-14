namespace QryloSocketAPI.Services;

public interface IMessagesService
{
    Task SendMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, string message, bool isAction);

    Task DeleteMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId,
        long messageCreatedOn);

    Task UpdateMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId,
        long messageCreatedOn, string message);

    Task ReadMessage(Guid memberMessageId, long messageCreatedOn, Guid memberId, long memberCreatedOn);

    Task ReportMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId,
        long messageCreatedOn, string comment);

    Task PinMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId,
        long messageCreatedOn);
    
    Task MarkAsDelivered(Guid memberMessageId, long messageCreatedOn, Guid memberId, long memberCreatedOn);
}