namespace SnowflakeId.AutoRegister.Core;

public class AppConst
{
    internal const string WorkerIdKeyPrefix = "WorkerId:";

    internal static string WorkerIdFormat(long workerId)
    {
        return WorkerIdKeyPrefix + workerId;
    }
}