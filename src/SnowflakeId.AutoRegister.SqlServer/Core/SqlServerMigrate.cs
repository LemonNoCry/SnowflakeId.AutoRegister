using System.Data.Common;
using Dapper;
using SnowflakeId.AutoRegister.SqlServer.Configs;
using SnowflakeId.AutoRegister.SqlServer.Resources;

namespace SnowflakeId.AutoRegister.SqlServer.Core;

public class SqlServerMigrate
{
    public static void Migrate(DbConnection connection, string schema)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));
        if (string.IsNullOrWhiteSpace(schema))
        {
            schema = SqlServerStorageOptions.DefaultSchema;
        }

        var migrateScript = ResourceManager.MigrateScript
           .Replace("$(SnowflakeSchema)", schema);
        connection.Execute(migrateScript);
    }
}