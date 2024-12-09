using System.Diagnostics;

namespace SnowflakeId.AutoRegister.Tests.Base;

public static class TestLog
{
    public static Action<LogLevel, string, Exception?>? GetLogAction(this ITestOutputHelper? testOutputHelper)
    {
        if (testOutputHelper is null) return default;

        return (logLevel, message, exception) =>
        {
            try
            {
                testOutputHelper.WriteLine($"[{Environment.CurrentManagedThreadId}]{logLevel}: {message} {Environment.NewLine} {exception}");
            }
            catch (Exception ex)
            {
                // Fallback to console or another logging mechanism
                Trace.WriteLine($"Logging failed: {ex.Message}");
            }
        };
    }
}