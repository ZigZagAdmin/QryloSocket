namespace QryloSocketAPI.Models;

public record Conversation
(
    Guid conversationId,
    
    string name,
    
    long createdOn,
    
    bool isPrivate
);