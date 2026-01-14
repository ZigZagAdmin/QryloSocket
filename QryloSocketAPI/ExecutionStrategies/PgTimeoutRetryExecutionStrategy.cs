using Microsoft.EntityFrameworkCore.Storage;

namespace QryloSocketAPI.ExecutionStrategies;

public class PgTimeoutRetryExecutionStrategy: ExecutionStrategy
{
    public PgTimeoutRetryExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies, maxRetryCount: 1, maxRetryDelay: TimeSpan.FromSeconds(1)) { }

    protected override bool ShouldRetryOn(Exception exception)
    {
        if (exception is TimeoutException) return true;

        if (exception is Npgsql.NpgsqlException { InnerException: System.IO.IOException or System.Net.Sockets.SocketException }) return true;

        if (exception is not Npgsql.PostgresException pgEx) return false;
        
        return pgEx.SqlState switch
        {
            "40001" or "40P01" or "53300" or "57P03" => true,
            _ => false
        };
    }

    protected override TimeSpan? GetNextDelay(Exception lastException)
    {
        return TimeSpan.FromMilliseconds(500);
    }
}