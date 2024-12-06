using SnowflakeId.AutoRegister.SqlServer.Resources;

namespace SnowflakeId.AutoRegister.SqlServer.Core;

public class SqlServerMigrate
{
    internal static string GetMigrateScript(string? schema = default)
    {
        if (string.IsNullOrWhiteSpace(schema))
        {
            schema = BaseDbStorageOptions.DefaultSchema;
        }

        return ResourceManager.MigrateScript
           .Replace("$(SnowflakeSchema)", schema);
    }

    public static void Migrate(DbConnection connection, string? schema = default)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));
        connection.Execute(GetMigrateScript(schema));
    }
}