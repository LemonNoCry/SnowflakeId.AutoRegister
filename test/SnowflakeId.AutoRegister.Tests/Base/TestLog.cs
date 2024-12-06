namespace SnowflakeId.AutoRegister.Tests.Base;

public static class TestLog
{
    public static Action<LogLevel, string, Exception?>? GetLogAction(this ITestOutputHelper? testOutputHelper)
    {
        if (testOutputHelper is null) return default;

        return (logLevel, message, exception) => { testOutputHelper.WriteLine($"{logLevel}: {message} {Environment.NewLine} {exception}"); };
    }
}