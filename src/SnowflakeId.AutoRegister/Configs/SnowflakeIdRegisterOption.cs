namespace SnowflakeId.AutoRegister.Configs;

/// <summary>
/// Represents the options for registering a SnowflakeId.
/// </summary>
public class SnowflakeIdRegisterOption
{
    /// <summary>
    /// Gets or sets the maximum number of loop iterations. The default value is 3. The minimum value is 1.
    /// </summary>
    public int MaxLoopCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the lifetime of a WorkerId in milliseconds. The default value is 30000 (30 seconds).<br/>
    /// Don't set it too low, otherwise, the WorkerId may be released before the WorkerId is actually expired.
    /// </summary>
    public int WorkerIdLifeMillisecond { get; set; } = 1000 * 30;

    /// <summary>
    /// Gets or sets the sleep duration in milliseconds for each loop iteration. The default value is 500.
    /// </summary>
    public int SleepMillisecondEveryLoop { get; set; } = 500;

    /// <summary>
    /// Gets or sets the minimum WorkerId. The default value is 1.
    /// </summary>
    public int MinWorkerId { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum WorkerId. The default value is 50.
    /// </summary>
    public int MaxWorkerId { get; set; } = 50;

    /// <summary>
    /// Gets or sets the identifier for the SnowflakeId. The default value is null. <br/>
    /// It is recommended to set a value to distinguish different SnowflakeId instances.<br/>
    /// For example, the server name, the server url, etc.
    /// Default to using Mac address and append extra identifiers.<br/>
    /// <br/>
    /// Use MD5(MAC+ExtraIdentifier) to generate the identifier.
    /// </summary>
    public string? ExtraIdentifier { get; set; }

    public void Validate()
    {
        if (MaxLoopCount < 1)
        {
            throw new ArgumentException("MaxLoopCount must be greater than or equal to 1");
        }

        if (WorkerIdLifeMillisecond < 900)
        {
            throw new ArgumentException("WorkerIdLifeMillisecond must be greater than or equal to 900");
        }

        if (MinWorkerId < 0)
        {
            throw new ArgumentException("MinWorkerId must be greater than or equal to 0");
        }

        if (MaxWorkerId < 0)
        {
            throw new ArgumentException("MaxWorkerId must be greater than or equal to 0");
        }

        if (MinWorkerId > MaxWorkerId)
        {
            throw new ArgumentException("MinWorkerId must be less than or equal to MaxWorkerId");
        }
    }
}