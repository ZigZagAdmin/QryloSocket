using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Entities;

[Table("ConversationMembers"), PrimaryKey("ConversationMemberId", "CreatedOn")]
public record ConversationMember
{
    public Guid ConversationMemberId { get; init; }
    
    public Guid ConversationId { get; init; }
    
    public long ConversationCreatedOn { get; init; }
    
    public Guid UserId { get; init; }
    
    public long UserCreatedOn { get; init; }
    
    public ConversationPermissions Permission { get; set; }
    
    public short IsBlocked { get; set; }
    
    public long CreatedOn { get; init; }
    
    public Guid CreatedBy { get; init; }
    
    public long CreatedByCreatedOn { get; init; }
}