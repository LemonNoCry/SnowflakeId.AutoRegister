using System;
using Microsoft.Data.SqlClient;
using SnowflakeId.AutoRegister.Db.Configs;

namespace SnowflakeId.AutoRegister.Tests.SqlServer;

public class SqlServerTestConfig
{
    public const string ConnectionString = "Server=localhost;Database=SnowflakeTest;Integrated Security=SSPI;TrustServerCertificate=true;";
}

public class SqlServerFixture : IDisposable
{
    public SqlServerFixture()
    {
        Connection = new SqlConnection(ConnectionString);
        Connection.Open();
    }

    public SqlConnection Connection { get; }

    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
    }

    private bool DoesTableExist(string tableName, string schemaName = "dbo")
    {
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = """
                    SELECT COUNT(*) 
                    FROM sys.tables t
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE t.name = @TableName 
                      AND s.name = @SchemaName
            """;
        cmd.Parameters.AddWithValue("@SchemaName", schemaName);
        cmd.Parameters.AddWithValue("@TableName", tableName);

        var count = Convert.ToInt32(cmd.ExecuteScalar());
        return count > 0;
    }

    public void ClearSnowflakeTable()
    {
        var tableName = $"[{BaseDbStorageOptions.DefaultSchema}].[RegisterKeyValues]";
        if (!DoesTableExist(tableName, BaseDbStorageOptions.DefaultSchema)) return;

        using var cmd = Connection.CreateCommand();
        cmd.CommandText = $"TRUNCATE TABLE [{BaseDbStorageOptions.DefaultSchema}].[RegisterKeyValues]";
        cmd.ExecuteNonQuery();
    }

    public int GetCount()
    {
        var tableName = $"[{BaseDbStorageOptions.DefaultSchema}].[RegisterKeyValues]";
        if (!DoesTableExist(tableName, BaseDbStorageOptions.DefaultSchema)) return 0;

        using var cmd = Connection.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM [{BaseDbStorageOptions.DefaultSchema}].[RegisterKeyValues]";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}