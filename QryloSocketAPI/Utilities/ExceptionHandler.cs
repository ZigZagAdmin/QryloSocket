using System.Diagnostics;
using Serilog;

namespace QryloSocketAPI.Utilities;

public class ExceptionHandler
{
    public static Models.Exception Exception(Exception ex)
    {
        if (ex.InnerException != null)
        {
            Log.Error(ex.InnerException, $"{ex.InnerException.Message} InnerException:");
        }
        
        Log.Error(ex, $"{ex.Message} Exception:");

        var stackTrace = new StackTrace(ex, true);
    
        var frame = stackTrace.GetFrame(0);
        var lineNumber = frame?.GetFileLineNumber() ?? -1;

        var functionPath = new List<string>();

        foreach (var frameItem in stackTrace.GetFrames())
        {
            var methodItem = frameItem.GetMethod();
            if (methodItem == null) continue;
            var declaringType = methodItem.DeclaringType?.FullName;
            var methodName = methodItem.Name;
            functionPath.Add($"{declaringType}.{methodName}");
        }

        return new Models.Exception(ex.Message, string.Join(" -> ", functionPath), lineNumber, ex.StackTrace ?? string.Empty);
    }
}