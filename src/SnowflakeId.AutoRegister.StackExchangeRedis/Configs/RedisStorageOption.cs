namespace SnowflakeId.AutoRegister.StackExchangeRedis.Configs;

/// <summary>
/// Represents the options for configuring Redis in the SnowflakeId AutoRegister system.
/// </summary>
public class RedisStorageOption
{
    public const string DefaultInstanceName = "SnowflakeId:";

    /// <summary>
    /// Gets or sets the configuration used to connect to Redis.
    /// </summary>
    public ConfigurationOptions? ConfigurationOptions { get; set; }

    /// <summary>
    /// Gets or sets a delegate to create the ConnectionMultiplexer instance.
    /// </summary>
    public Func<IConnectionMultiplexer>? ConnectionMultiplexerFactory { get; set; }

    /// <summary>
    /// Gets or sets the Redis instance name. The default value is "SnowflakeId:".
    /// </summary>
    public string? InstanceName { get; set; } = DefaultInstanceName;
}