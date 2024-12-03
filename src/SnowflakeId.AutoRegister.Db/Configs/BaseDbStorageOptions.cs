using System.Data.Common;

namespace SnowflakeId.AutoRegister.Db.Configs;

/// <summary>
/// Represents the base options for configuring a database storage mechanism in the SnowflakeId AutoRegister system.
/// </summary>
public abstract class BaseDbStorageOptions
{
    public const string DefaultSchema = "Snowflake";
    private DbProviderFactory? _sqlClientFactory;

    /// <summary>
    /// Gets or sets the name of the schema in the database.
    /// </summary>
    public string SchemaName = DefaultSchema;

    /// <summary>
    /// Gets or sets the connection string used to connect to the database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the <see cref="DbProviderFactory"/> for creating <c>DbConnection</c> instances.
    /// </summary>
    public DbProviderFactory SqlClientFactory
    {
        get
        {
            _sqlClientFactory ??= GetDefaultSqlClientFactory();

            return _sqlClientFactory ?? throw new InvalidOperationException("The SQL client factory could not be resolved.");
        }
        set => _sqlClientFactory = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the factory method to create a new connection to the database.
    /// </summary>
    public Func<DbConnection>? ConnectionFactory { get; set; }

    protected abstract DbProviderFactory GetDefaultSqlClientFactory();

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