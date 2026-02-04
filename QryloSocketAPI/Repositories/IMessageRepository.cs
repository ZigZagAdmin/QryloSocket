using QryloSocketAPI.Models;

namespace QryloSocketAPI.Repositories;

public interface IMessageRepository
{
    Task Create(Guid conversationId, Guid userId, List<MessagePart> parts, bool isAction);

    Task Update(Guid messageId, Guid conversationId, Guid requesterId, List<MessagePart> parts);
    
    Task Delete(Guid messageId, Guid conversationId, Guid requesterId, bool isAdmin);
    
    Task Read(Guid memberMessageId, Guid memberId);

    Task Pin(Guid messageId, Guid conversationId, Guid requesterId);
    
    Task Report(Guid messageId, Guid conversationId, Guid requesterId);
}