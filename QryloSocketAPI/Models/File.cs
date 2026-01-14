namespace QryloSocketAPI.Models;

public record File
{
    public string Link { get; set; }
    
    public string Name { get; set; }
    
    public string Content { get; set; }
    
    public string Type { get; set; } 
}