using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Models;

public record Member
(
    Guid memberId, 
    
    string name, 
    
    ConversationPermissions permission
);