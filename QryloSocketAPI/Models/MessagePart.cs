using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Models;

public record MessagePart
(
    MessagePartTypes Type, 
    
    int Order, 
    
    string Content
);