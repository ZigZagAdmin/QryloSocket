namespace QryloSocketAPI.Models;

public record Exception
(
    string Message,
    
    string Path,
    
    int Line ,
    
    string StackTrace
);