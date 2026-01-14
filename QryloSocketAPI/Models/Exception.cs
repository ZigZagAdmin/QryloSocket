namespace QryloSocketAPI.Models;

public record Exception
(
    string message,
    string path,
    int line ,
    string stackTrace
);