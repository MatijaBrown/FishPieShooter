using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace FishPieShooter.Utils;

public static class Log
{
    
    public static ILogger CoreLogger { get; private set; } = null!;

    public static ILogger ClientLogger { get; private set; } = null!;

    public static void Init()
    {
        CoreLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] Fish Pie: {Message}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code
            ).CreateLogger();
        ClientLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] Client: {Message}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code
            ).CreateLogger();
    }
    
    // Core Log Macros
    public static void CoreDebug(string messageTemplate)
        => CoreLogger.Debug(messageTemplate);
    
    public static void CoreDebug(string messageTemplate, params object[]? propertyValues)
        => CoreLogger.Debug(messageTemplate, propertyValues);
    
    public static void CoreDebug(Exception? exception, string messageTemplate)
        => CoreLogger.Debug(exception, messageTemplate);
    
    public static void CoreDebug(Exception? exception, string messageTemplate, params object[]? propertyValues)
        => CoreLogger.Debug(exception, messageTemplate, propertyValues);
    
    public static void CoreInfo(string messageTemplate)
        => CoreLogger.Information(messageTemplate);
    
    public static void CoreInfo(string messageTemplate, params object[]? propertyValues)
        => CoreLogger.Information(messageTemplate, propertyValues);
    
    public static void CoreInfo(Exception? exception, string messageTemplate)
        => CoreLogger.Information(exception, messageTemplate);
    
    public static void CoreInfo(Exception? exception, string messageTemplate, params object[]? propertyValues)
        => CoreLogger.Information(exception, messageTemplate, propertyValues);
    
    public static void CoreWar(string messageTemplate)
        => CoreLogger.Warning(messageTemplate);
    
    public static void CoreWarn(string messageTemplate, params object[]? propertyValues)
        => CoreLogger.Warning(messageTemplate, propertyValues);
    
    public static void CoreWarn(Exception? exception, string messageTemplate)
        => CoreLogger.Warning(exception, messageTemplate);
    
    public static void CoreWarn(Exception? exception, string messageTemplate, params object[]? propertyValues)
        => CoreLogger.Warning(exception, messageTemplate, propertyValues);
    
    public static void CoreError(string messageTemplate)
        => CoreLogger.Error(messageTemplate);
    
    public static void CoreError(string messageTemplate, params object[]? propertyValues)
        => CoreLogger.Error(messageTemplate, propertyValues);
    
    public static void CoreError(Exception? exception, string messageTemplate)
        => CoreLogger.Error(exception, messageTemplate);
    
    public static void CoreError(Exception? exception, string messageTemplate, params object[]? propertyValues)
        => CoreLogger.Error(exception, messageTemplate, propertyValues);
    
    // Client Log Macros
    
    public static void ClientDebug(string messageTemplate)
        => ClientLogger.Debug(messageTemplate);
    
    public static void ClientDebug(string messageTemplate, params object[]? propertyValues)
        => ClientLogger.Debug(messageTemplate, propertyValues);
    
    public static void ClientDebug(Exception? exception, string messageTemplate)
        => ClientLogger.Debug(exception, messageTemplate);
    
    public static void ClientDebug(Exception? exception, string messageTemplate, params object[]? propertyValues)
        => ClientLogger.Debug(exception, messageTemplate, propertyValues);
    
    public static void ClientInfo(string messageTemplate)
        => ClientLogger.Information(messageTemplate);
    
    public static void ClientInfo(string messageTemplate, params object[]? propertyValues)
        => ClientLogger.Information(messageTemplate, propertyValues);
    
    public static void ClientInfo(Exception? exception, string messageTemplate)
        => ClientLogger.Information(exception, messageTemplate);
    
    public static void ClientInfo(Exception? exception, string messageTemplate, params object[]? propertyValues)
        => ClientLogger.Information(exception, messageTemplate, propertyValues);
    
    public static void ClientWar(string messageTemplate)
        => ClientLogger.Warning(messageTemplate);
    
    public static void ClientWarn(string messageTemplate, params object[]? propertyValues)
        => ClientLogger.Warning(messageTemplate, propertyValues);
    
    public static void ClientWarn(Exception? exception, string messageTemplate)
        => ClientLogger.Warning(exception, messageTemplate);
    
    public static void ClientWarn(Exception? exception, string messageTemplate, params object[]? propertyValues)
        => ClientLogger.Warning(exception, messageTemplate, propertyValues);
    
    public static void ClientError(string messageTemplate)
        => ClientLogger.Error(messageTemplate);
    
    public static void ClientError(string messageTemplate, params object[]? propertyValues)
        => ClientLogger.Error(messageTemplate, propertyValues);
    
    public static void ClientError(Exception? exception, string messageTemplate)
        => ClientLogger.Error(exception, messageTemplate);
    
    public static void ClientError(Exception? exception, string messageTemplate, params object[]? propertyValues)
        => ClientLogger.Error(exception, messageTemplate, propertyValues);
    
}