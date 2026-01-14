// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QryloSocketAPI;
using QryloSocketAPI.Database;
using QryloSocketAPI.Extensions;
using QryloSocketAPI.Utilities;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.EnableDbCommandCaching", false);
LoggerExtension.ConfigureLogger();
try
{
    var host = CreateHostBuilder(args).Build();
    await WarmUpDatabase(host);
    ShutdownHandler.RegisterShutdownHandling(host.Services.GetRequiredService<IConfiguration>());
    await host.RunAsync();
    Log.Information("Starting Qrylo Socket API");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Qrylo Socket API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

static async Task WarmUpDatabase(IHost host)
{
    using var scope = host.Services.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<QryloContext>>();
    await using var context = await factory.CreateDbContextAsync();
    await context.Database.OpenConnectionAsync();
    await context.Database.CloseConnectionAsync();
}

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });