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

/// <summary>
/// Used to build the Snowflake ID library <br />
/// Defines an interface for automatic registration.
/// </summary>
/// <typeparam name="T">The type of the SnowflakeId generator.</typeparam>
public interface IAutoRegister<out T> : IDisposable where T : class
{
    /// <summary>
    /// Retrieves the configuration for the SnowflakeId generator.
    /// This method should be called after <see cref="GetIdGenerator" /> to obtain the relevant configuration.
    /// </summary>
    /// <returns>A <see cref="SnowflakeIdConfig" /> instance, or null if the generator is not initialized.</returns>
    SnowflakeIdConfig? GetSnowflakeIdConfig();

    /// <summary>
    /// Provides an instance of the SnowflakeId generator.
    /// </summary>
    /// <returns>An instance of the SnowflakeId generator of type <typeparamref name="T" />.</returns>
    T GetIdGenerator();

    /// <summary>
    /// Unregisters the SnowflakeIdConfig.
    /// </summary>
    void UnRegisterIdGenerator();
}