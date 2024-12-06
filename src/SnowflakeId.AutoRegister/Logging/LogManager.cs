namespace SnowflakeId.AutoRegister.Logging;

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Fatal
}

internal sealed class LogManager
{
    private static readonly Lazy<LogManager> _instance = new(() => new LogManager());

    private LogManager()
    {
    }

    private LogLevel MinimumLogLevel { get; set; } = LogLevel.Trace;
    private Action<LogLevel, string, Exception?>? LogAction { get; set; }

    public static LogManager Instance => _instance.Value;

    public LogManager SetLogAction(Action<LogLevel, string, Exception?>? logAction)
    {
        LogAction = logAction;
        return this;
    }

    public LogManager SetMinimumLogLevel(LogLevel minimumLogLevel)
    {
        MinimumLogLevel = minimumLogLevel;
        return this;
    }

    #region Log Methods

    internal void LogTrace(string message)
    {
        if (MinimumLogLevel > LogLevel.Trace) return;

        LogAction?.Invoke(LogLevel.Trace, message, default);
    }

    internal void LogDebug(string message)
    {
        if (MinimumLogLevel > LogLevel.Debug) return;

        LogAction?.Invoke(LogLevel.Debug, message, default);
    }

    internal void LogInfo(string message)
    {
        if (MinimumLogLevel > LogLevel.Info) return;

        LogAction?.Invoke(LogLevel.Info, message, default);
    }

    internal void LogWarn(string message)
    {
        if (MinimumLogLevel > LogLevel.Warn) return;

        LogAction?.Invoke(LogLevel.Warn, message, default);
    }

    internal void LogError(string message, Exception? exception = default)
    {
        if (MinimumLogLevel > LogLevel.Error) return;

        LogAction?.Invoke(LogLevel.Error, message, exception);
    }

    internal void LogFatal(string message, Exception? exception = default)
    {
        if (MinimumLogLevel > LogLevel.Fatal) return;

        LogAction?.Invoke(LogLevel.Fatal, message, exception);
    }

    #endregion
}