using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using QryloSocketAPI.Utilities.Enums;

namespace QryloSocketAPI.Entities;

[Table("MessageParts"), PrimaryKey("MessagePartId", "CreatedOn")]

public record MessagePart
{
    public Guid MessagePartId { get; init; }
    
    public Guid MessageId { get; init; }
    
    public long MessageCreatedOn { get; init; }
    
    public MessagePartTypes Type { get; init; }
    
    public int Order { get; init; }
    
    public string Content { get; init; }
    
    public long CreatedOn { get; init; }
}