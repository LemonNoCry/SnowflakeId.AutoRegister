namespace SnowflakeId.AutoRegister.Interfaces;

/// <summary>
/// Defines an interface for automatic registration.
/// </summary>
public interface IAutoRegister : IDisposable
{
    /// <summary>
    /// Register a SnowflakeIdConfig.
    /// </summary>
    /// <returns>A SnowflakeIdConfig instance.</returns>
    SnowflakeIdConfig Register();

    /// <summary>
    /// UnRegister the SnowflakeIdConfig.
    /// </summary>
    void UnRegister();
}