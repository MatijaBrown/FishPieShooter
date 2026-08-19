using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace FishPieClient.Utils;

public static class Logging
{

    public static void InitLogger()
    {
        var config = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] Fish Pie: {Message}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code
        );

        config = ProjectUtils.LoggingEnabled ? config.MinimumLevel.Debug() : config.MinimumLevel.Error();
        
        if (ProjectUtils.LogToFile)
        {
            
        }

        Log.Logger = config.CreateLogger();
    }
    
}