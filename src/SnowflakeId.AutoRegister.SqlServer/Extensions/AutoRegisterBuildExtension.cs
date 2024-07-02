using System.Data.Common;
using SnowflakeId.AutoRegister.SqlServer.Configs;
using SnowflakeId.AutoRegister.SqlServer.Storage;

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
    public static AutoRegisterBuilder UseSqlServerStore(this AutoRegisterBuilder builder, Action<SqlServerStorageOptions> redisOption)
    {
        if (redisOption is null) throw new ArgumentNullException(nameof(redisOption));

        var option = new SqlServerStorageOptions();
        redisOption(option);
        return builder.UseStore(new SqlServerStorage(option));
    }

    /// <summary>
    /// Configures the AutoRegisterBuilder to use SQL Server as the storage mechanism with a connection string.
    /// </summary>
    /// <param name="builder">The AutoRegisterBuilder to be configured.</param>
    /// <param name="connectionString">The connection string to the SQL Server database.</param>
    /// <returns>The configured AutoRegisterBuilder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the connectionString parameter is null or empty.</exception>
    public static AutoRegisterBuilder UseSqlServerStore(this AutoRegisterBuilder builder, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        return UseSqlServerStore(builder, option => option.ConnectionString = connectionString);
    }

    /// <summary>
    /// Configures the AutoRegisterBuilder to use SQL Server as the storage mechanism with a connection factory method.
    /// </summary>
    /// <param name="builder">The AutoRegisterBuilder to be configured.</param>
    /// <param name="connectionFactory">A function to create a new connection to the SQL Server database.</param>
    /// <returns>The configured AutoRegisterBuilder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the connectionFactory parameter is null.</exception>
    public static AutoRegisterBuilder UseSqlServerStore(this AutoRegisterBuilder builder, Func<DbConnection> connectionFactory)
    {
        if (connectionFactory is null) throw new ArgumentNullException(nameof(connectionFactory));

        return UseSqlServerStore(builder, option => option.ConnectionFactory = connectionFactory);
    }
}