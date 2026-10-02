using System.Numerics;
using Hexa.NET.ImGui;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace FishPieClient.Utils;

public static class Logging
{

    public static readonly ImGuiSink ImGuiSink = new();
    
    public static void InitLogger()
    {
        var config = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] Fish Pie: {Message:l}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code
            );
        config.WriteTo.Sink(ImGuiSink, LogEventLevel.Debug);

        config = ProjectUtils.LoggingEnabled ? config.MinimumLevel.Debug() : config.MinimumLevel.Error();
        
        if (ProjectUtils.LogToFile)
        {
            
        }

        Log.Logger = config.CreateLogger();
    }
    
}

public class ImGuiSink : ILogEventSink
{
    
    private readonly List<LogEvent> _history = [];
    
    public void DrawToImGui()
    {
        foreach (var logEvent in _history)
        {
            var log = $"[{logEvent.Timestamp:hh\\:mm\\:ss} {logEvent.Level}] {logEvent.RenderMessage()}";
            if (logEvent.Exception != null)
            {
                log += '\n' + logEvent.Exception.ToString();
            }

            switch (logEvent.Level)
            {
                case LogEventLevel.Fatal:
                case LogEventLevel.Error:
                    ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), log);
                    break;
                case LogEventLevel.Warning:
                    ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), log);
                    break;
                case LogEventLevel.Information:
                    ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), log);
                    break;
                case LogEventLevel.Debug:
                case LogEventLevel.Verbose:
                    ImGui.TextColored(new Vector4(0.0f, 0.5f, 1.0f, 1.0f), log);
                    break;
            }
        }
    }
    
    public void Emit(LogEvent logEvent)
    {
        _history.Add(logEvent);
    }
    
}