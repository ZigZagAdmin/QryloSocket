using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QryloSocketAPI.Entities;

[PrimaryKey("ConversationId", "CreatedOn")]
public record Conversations
{
    public Guid ConversationId { get; init; }

    public string Name { get; set; } = string.Empty;
    
    public string Avatar { get; set; } = string.Empty;
    
    public short IsPrivate { get; set; }
    
    public long CreatedOn { get; init; }
    
    public Guid CreatedBy { get; init; }
    
    public long CreatedByCreatedOn { get; init; }
}