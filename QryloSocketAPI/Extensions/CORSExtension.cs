using Microsoft.Extensions.DependencyInjection;

namespace QryloSocketAPI.Extensions;

public static class CORSExtension
{
    public static IServiceCollection AddCORS(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
            options.AddPolicy("AllowOrigin", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        return services;
    }
}