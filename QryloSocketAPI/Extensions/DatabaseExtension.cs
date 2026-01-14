using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QryloSocketAPI.Database;
using QryloSocketAPI.ExecutionStrategies;

namespace QryloSocketAPI.Extensions;

public static class DatabaseExtension
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPooledDbContextFactory<QryloContext>((sp, options) =>
        {
            options.UseNpgsql(
                    configuration.GetConnectionString("PostgresConnection"),
                    npgsqlOptions => npgsqlOptions.ExecutionStrategy(deps => new PgTimeoutRetryExecutionStrategy(deps))
                )
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .ConfigureWarnings(warnings =>
                    warnings.Throw(RelationalEventId.NonQueryOperationFailed));
        });
        return services;
    }
}