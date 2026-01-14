using Microsoft.EntityFrameworkCore;

namespace QryloSocketAPI.Entities;

[PrimaryKey("MessageId", "CreatedOn")]
public record Messages
{
    public Guid MessageId { get; init; }
    
    public Guid UserId { get; init; }
    
    public long UserCreatedOn { get; init; }
    
    public Guid ConversationId { get; init; }
    
    public long ConversationCreatedOn { get; init; }
    
    public string Text { get; set; } = string.Empty;
    
    public long UpdatedOn { get; set; }
    
    public long CreatedOn { get; init; }
    
    public short IsDelivered { get; set; }
    
    public short IsRead { get; set; }
}