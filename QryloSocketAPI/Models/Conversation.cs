namespace QryloSocketAPI.Models;

public record Conversation
(
    Guid ConversationId,
    
    string Name,
    
    long CreatedOn,
    
    bool IsPrivate
);