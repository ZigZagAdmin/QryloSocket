using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QryloSocketAPI.Entities;

[Table("Messages"), PrimaryKey("MessageId", "CreatedOn")]
public record Message
{
    public Guid MessageId { get; init; }
    
    public Guid UserId { get; init; }
    
    public long UserCreatedOn { get; init; }
    
    public Guid ConversationId { get; init; }
    
    public long ConversationCreatedOn { get; init; }
    
    public long UpdatedOn { get; set; }
    
    public long CreatedOn { get; init; }
    
    public short IsDelivered { get; set; }
    
    public short IsRead { get; set; }
}