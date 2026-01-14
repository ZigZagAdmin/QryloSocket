using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace QryloSocketAPI.Extensions;

public class LoggerExtension
{
    public static void ConfigureLogger()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDevelopment = string.Equals(environment, Environments.Development, StringComparison.OrdinalIgnoreCase);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", isDevelopment ? LogEventLevel.Debug : LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", isDevelopment ? LogEventLevel.Debug : LogEventLevel.Warning)
            .MinimumLevel.Override("System", isDevelopment ? LogEventLevel.Debug : LogEventLevel.Warning)
            .WriteTo.Console()
            .CreateLogger();
    }
}