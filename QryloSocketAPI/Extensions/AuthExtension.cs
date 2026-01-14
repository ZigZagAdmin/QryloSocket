using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QryloSocketAPI.Models;

namespace QryloSocketAPI.Extensions;

public static class AuthExtension
{
    public static void AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = configuration.GetSection("Auth").Get<AuthOptions>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience,

                    ValidateLifetime = true,

                    IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                };
            });
        services.AddAuthorization();
    }
}