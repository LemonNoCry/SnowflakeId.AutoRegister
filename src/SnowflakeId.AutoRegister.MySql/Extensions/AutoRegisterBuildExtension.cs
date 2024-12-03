using System.Data.Common;
using SnowflakeId.AutoRegister.MySql.Configs;
using SnowflakeId.AutoRegister.MySql.Storage;

// ReSharper disable once CheckNamespace
namespace SnowflakeId.AutoRegister.Builder;

public static class AutoRegisterBuildExtension
{
    /// <summary>
    /// Configures the AutoRegisterBuilder to use MySQL as the storage mechanism.
    /// </summary>
    /// <param name="builder">The AutoRegisterBuilder to be configured.</param>
    /// <param name="mysqlOption">An action to configure the MySQL options.</param>
    /// <returns>The configured AutoRegisterBuilder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the action parameter is null.</exception>
    public static AutoRegisterBuilder UseMySqlStore(this AutoRegisterBuilder builder, Action<MySqlStorageOptions> mysqlOption)
    {
        if (mysqlOption is null) throw new ArgumentNullException(nameof(mysqlOption));

        var option = new MySqlStorageOptions();
        mysqlOption(option);
        return builder.UseStore(new MySqlStorage(option));
    }

    /// <summary>
    /// Configures the AutoRegisterBuilder to use MySQL as the storage mechanism with a connection string.
    /// </summary>
    /// <param name="builder">The AutoRegisterBuilder to be configured.</param>
    /// <param name="connectionString">The connection string to the MySQL database.</param>
    /// <returns>The configured AutoRegisterBuilder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the connectionString parameter is null or empty.</exception>
    public static AutoRegisterBuilder UseMySqlStore(this AutoRegisterBuilder builder, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        return UseMySqlStore(builder, option => option.ConnectionString = connectionString);
    }

    /// <summary>
    /// Configures the AutoRegisterBuilder to use MySQL as the storage mechanism with a connection factory method.
    /// </summary>
    /// <param name="builder">The AutoRegisterBuilder to be configured.</param>
    /// <param name="connectionFactory">A function to create a new connection to the MySQL database.</param>
    /// <returns>The configured AutoRegisterBuilder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the connectionFactory parameter is null.</exception>
    public static AutoRegisterBuilder UseMySqlStore(this AutoRegisterBuilder builder, Func<DbConnection> connectionFactory)
    {
        if (connectionFactory is null) throw new ArgumentNullException(nameof(connectionFactory));

        return UseMySqlStore(builder, option => option.ConnectionFactory = connectionFactory);
    }
}