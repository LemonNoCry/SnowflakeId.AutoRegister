using System.Data.Common;

namespace SnowflakeId.AutoRegister.SqlServer.Configs;

/// <summary>
/// Represents the options for configuring SQL Server in the SnowflakeId AutoRegister system.
/// </summary>
public class SqlServerStorageOptions
{
    public const string DefaultSchema = "Snowflake";

    /// <summary>
    /// Gets or sets the connection string used to connect to the SQL Server database.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the schema in the SQL Server database.
    /// </summary>
    public string SchemaName { get; set; } = DefaultSchema;

    /// <summary>
    /// Gets or sets the factory method to create a new connection to the SQL Server database.
    /// </summary>
    public Func<DbConnection>? ConnectionFactory { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString) && ConnectionFactory is null)
        {
            throw new ArgumentNullException(nameof(ConnectionString),
                "The connection string cannot be null or empty. Please provide a valid connection string or a connection factory method.");
        }

        if (string.IsNullOrWhiteSpace(SchemaName))
        {
            throw new ArgumentNullException(nameof(SchemaName));
        }
    }
}