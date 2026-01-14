using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace QryloSocketAPI.Models;

public record AuthOptions
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string Secret { get; set; }
    public int TokenLifeTime { get; set; }
    
    public int TokenLifeTimeRecovery { get; set; }

    public SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));
    }
}