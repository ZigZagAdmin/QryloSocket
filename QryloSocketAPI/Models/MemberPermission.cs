using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Models;

public record MemberPermission(
    
    Guid MemberId,
    
    ConversationPermissions[] Permissions
    
    );