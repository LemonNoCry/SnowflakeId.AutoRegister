using SnowflakeId.AutoRegister.StackExchangeRedis.Configs;
using SnowflakeId.AutoRegister.StackExchangeRedis.Storage;

// ReSharper disable once CheckNamespace
namespace SnowflakeId.AutoRegister.Builder;

public static partial class AutoRegisterBuildExtension
{
    /// <summary>
    /// Configures the AutoRegisterBuilder to use Redis as the storage mechanism.
    /// </summary>
    /// <param name="builder">The AutoRegisterBuilder to be configured.</param>
    /// <param name="redisOption">An action to configure the Redis options.</param>
    /// <returns>The configured AutoRegisterBuilder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the action parameter is null.</exception>
    public static AutoRegisterBuilder UseRedisStore(this AutoRegisterBuilder builder, Action<RedisStorageOption> redisOption)
    {
        if (redisOption is null) throw new ArgumentNullException(nameof(redisOption));

        var option = new RedisStorageOption();
        redisOption(option);
        return builder.UseStore(new RedisStorage(option));
    }

    /// <summary>
    /// Configures the AutoRegisterBuilder to use Redis as the storage mechanism.
    /// </summary>
    /// <param name="builder">The AutoRegisterBuilder to be configured.</param>
    /// <param name="configuration">The configuration used to connect to Redis.</param>
    /// <returns>The configured AutoRegisterBuilder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configuration parameter is null or empty.</exception>
    public static AutoRegisterBuilder UseRedisStore(this AutoRegisterBuilder builder, string configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration)) throw new ArgumentNullException(nameof(configuration));

        return UseRedisStore(builder, option => option.ConfigurationOptions = ConfigurationOptions.Parse(configuration));
    }

    /// <summary>
    /// Configures the AutoRegisterBuilder to use Redis as the storage mechanism.
    /// </summary>
    /// <param name="builder">The AutoRegisterBuilder to be configured.</param>
    /// <param name="connectionMultiplexerFactory">A delegate to create the ConnectionMultiplexer instance.</param>
    /// <returns>The configured AutoRegisterBuilder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the connectionMultiplexerFactory parameter is null.</exception>
    public static AutoRegisterBuilder UseRedisStore(this AutoRegisterBuilder builder, Func<IConnectionMultiplexer> connectionMultiplexerFactory)
    {
        if (connectionMultiplexerFactory is null) throw new ArgumentNullException(nameof(connectionMultiplexerFactory));

        return UseRedisStore(builder, option => option.ConnectionMultiplexerFactory = connectionMultiplexerFactory);
    }
}