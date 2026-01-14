namespace QryloSocketAPI.Models;

public record UserConnection
{
    public string UserId { get; set; } = string.Empty;
    
    public string ConnectionId { get; set; } = string.Empty;
}