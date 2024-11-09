namespace SnowflakeId.AutoRegister.Configs;

/// <summary>
/// Represents the configuration for a SnowflakeId.
/// </summary>
public class SnowflakeIdConfig
{
    /// <summary>
    /// Gets the WorkerId for the SnowflakeId.
    /// </summary>
    public int WorkerId { get; internal set; }

    /// <summary>
    /// Gets the identifier for the SnowflakeId.
    /// </summary>
    public string Identifier { get; internal set; } = string.Empty;
}