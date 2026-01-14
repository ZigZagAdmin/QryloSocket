using System.Reflection;
using System.Text.Json;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QryloSocketAPI.Extensions;
using QryloSocketAPI.Middleware;
using Serilog;
using StackExchange.Redis;

namespace QryloSocketAPI;

public class Startup(IConfiguration configuration)
{
    private bool _redisConnectionFailed;

    public void ConfigureServices(IServiceCollection services)
    {
        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            if (_redisConnectionFailed)
            {
                Log.Warning("Skipping Redis connection as it has failed previously.");
                return null!;
            }

            try
            {
                var connection =
                    ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? string.Empty);
                Log.Information("Connection with Redis successfully established!");
                return connection;
            }
            catch (RedisConnectionException ex)
            {
                Log.Warning("Could not connect to Redis!");
                Log.Warning(ex, "Exception");
                _redisConnectionFailed = true;
                return null!;
            }
        });
        // EF DbContext
        services.AddDatabase(configuration);
        // Auth
        services.AddHttpContextAccessor();
        services.AddAuth(configuration);
        // CORS
        services.AddCORS();
        services.AddHttpClient();
        services.AddHealthChecks();
        services.AddDependencies();
        services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.HandshakeTimeout = TimeSpan.FromMinutes(2);
                options.KeepAliveInterval = TimeSpan.FromMinutes(2);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
            })
            .AddStackExchangeRedis(configuration.GetConnectionString("Redis") ?? string.Empty, options =>
            {
                options.Configuration.ChannelPrefix = "qrylo-signalr";
                options.Configuration.AbortOnConnectFail = false;
                options.Configuration.ConnectRetry = 3;
                options.Configuration.ConnectTimeout = 5000;
            }).AddJsonProtocol(options => { options.PayloadSerializerOptions.PropertyNamingPolicy = null; });
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseWebSockets();
        app.UseRouting();
        app.UseCors("AllowOrigin");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        });
        app.UseAntiXssMiddleware();
    }
}