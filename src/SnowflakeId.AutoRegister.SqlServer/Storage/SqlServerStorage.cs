using SnowflakeId.AutoRegister.Db.Storage;
using SnowflakeId.AutoRegister.SqlServer.Configs;
using SnowflakeId.AutoRegister.SqlServer.Core;

namespace SnowflakeId.AutoRegister.SqlServer.Storage;

/// <summary>
/// Represents a SQL Server storage mechanism for the SnowflakeId AutoRegister system.
/// </summary>
public class SqlServerStorage : BaseDbStorage
{
    private readonly SqlServerStorageOptions _options;

    internal SqlServerStorage(SqlServerStorageOptions option) : base(option, new SqlServerQueryProvider())
    {
        _options = option ?? throw new ArgumentNullException(nameof(option));
        _options.Validate();

        Initialize();
    }

    private void Initialize()
    {
        for (var index = 0; index < 3; ++index)
        {
            try
            {
                UseConnection(connection => SqlServerMigrate.Migrate(connection, this._options.SchemaName));
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