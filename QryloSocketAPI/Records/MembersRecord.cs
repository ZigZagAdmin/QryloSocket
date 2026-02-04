using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Records;

public record MembersRecord
{
    public Guid ConversationMemberId { get; set; }
    
    public Guid MemberId { get; set; }

    public string Name { get; set; }

    public ConversationPermissions Permission { get; set; }
    
    public bool IsBlocked { get; set; }
}