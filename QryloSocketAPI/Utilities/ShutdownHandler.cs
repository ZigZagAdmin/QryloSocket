using Microsoft.Extensions.Configuration;

namespace QryloSocketAPI.Utilities;

public class ShutdownHandler
{
    private static volatile bool _isShuttingDown;
    
    public static bool IsShuttingDown => _isShuttingDown;
    
    private static int _gracePeriodMilliseconds = 10000;

    public static void RegisterShutdownHandling(IConfiguration configuration)
    {
        var setting = configuration["ShutdownGracePeriodMilliseconds"];
        if (int.TryParse(setting, out var parsed))
        {
            _gracePeriodMilliseconds = parsed;
        }
        
        AppDomain.CurrentDomain.ProcessExit += (_, __) =>
        {
            _isShuttingDown = true;
            Thread.Sleep(_gracePeriodMilliseconds);
        };
    }
}