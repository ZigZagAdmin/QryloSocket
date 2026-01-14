namespace QryloSocketAPI.Repositories;

public interface IMessagesRepository
{
    Task Create(Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn, string text, bool isAction);

    Task Update(Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn, Guid userId,
        long userCreatedOn, string text);
    
    Task Delete(Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn);
    
    Task Read(Guid memberMessageId, long messageCreatedOn, Guid memberId, long memberCreatedOn);

    Task MarkAsDelivered(Guid memberMessageId, long messageCreatedOn, Guid memberId, long memberCreatedOn);
    
    Task Pin(Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn);
    
    Task Report(Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn, Guid userId, long userCreatedOn);
}