namespace SnowflakeId.AutoRegister.SqlServer.Storage;

public class SqlServerQueryProvider : ISqlQueryProvider
{
    public string GetExistQuery(string schemaName) =>
        $"SELECT TOP 1 1 FROM [{schemaName}].[RegisterKeyValues] WHERE [Key] = @key";

    public string GetQuery(string schemaName) =>
        $"SELECT VALUE FROM [{schemaName}].[RegisterKeyValues] WHERE [Key] = @key";

    public string GetInsertOrUpdateQuery(string schemaName) =>
        $"""
        MERGE INTO [{schemaName}].[RegisterKeyValues] AS Target
        USING (VALUES (@key, @value, @expireTime)) AS Source ([Key], Value, ExpireTime)
        ON Target.[Key] = Source.[Key]
        WHEN MATCHED THEN 
            UPDATE SET Value = Source.Value, ExpireTime = Source.ExpireTime
        WHEN NOT MATCHED THEN 
            INSERT ([Key], Value, ExpireTime) VALUES (Source.[Key], Source.Value, Source.ExpireTime);
        """;

    public string GetInsertIfNotExistsQuery(string schemaName) =>
        $"""
        INSERT INTO [{schemaName}].[RegisterKeyValues] ([Key], Value, ExpireTime)
        SELECT @key, @value, @expireTime
        WHERE NOT EXISTS (SELECT 1 FROM [{schemaName}].[RegisterKeyValues] WHERE [Key] = @key);
        """;

    public string GetUpdateExpireQuery(string schemaName) =>
        $"UPDATE [{schemaName}].[RegisterKeyValues] SET ExpireTime = @expireTime WHERE [Key] = @key";

    public string GetDeleteQuery(string schemaName) =>
        $"DELETE FROM [{schemaName}].[RegisterKeyValues] WHERE [Key] = @key";

    public string GetClearExpiredQuery(string schemaName) =>
        $"DELETE FROM [{schemaName}].[RegisterKeyValues] WHERE [Key] != @key AND ExpireTime < @now";
}