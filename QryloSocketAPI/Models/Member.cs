using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Models;

public record Member
(
    Guid ConversationMemberId, 
    
    Guid MemberId, 
    
    string Name, 
    
    ConversationPermissions[] Permission,
    
    bool IsBlocked
);