using Microsoft.EntityFrameworkCore;

namespace QryloSocketAPI.Entities;

[PrimaryKey("ConversationMemberId", "CreatedOn")]
public record ConversationMembers
{
    public Guid ConversationMemberId { get; init; }
    
    public Guid ConversationId { get; init; }
    
    public long ConversationCreatedOn { get; init; }
    
    public Guid UserId { get; init; }
    
    public long UserCreatedOn { get; init; }
    
    public int Permission { get; set; }
    
    public short IsBlocked { get; set; }
    
    public long CreatedOn { get; init; }
    
    public Guid CreatedBy { get; init; }
    
    public long CreatedByCreatedOn { get; init; }
}