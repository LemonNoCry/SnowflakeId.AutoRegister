using SnowflakeId.AutoRegister.MySql.Core;

namespace SnowflakeId.AutoRegister.MySql.Storage;

/// <summary>
/// Represents a MySQL storage mechanism for the SnowflakeId AutoRegister system.
/// </summary>
internal class MySqlStorage : BaseDbStorage
{
    private readonly MySqlStorageOptions _options;

    internal MySqlStorage(MySqlStorageOptions options) : base(options, new MySqlQueryProvider())
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        Initialize();
    }

    private void Initialize()
    {
        for (var index = 0; index < 3; ++index)
        {
            try
            {
                UseConnection(connection => MySqlMigrate.Migrate(connection, _options.SchemaName));
                return;
            }
            catch (Exception)
            {
                if (index == 2)
                {
                    throw;
                }
            }
        }
    }
}