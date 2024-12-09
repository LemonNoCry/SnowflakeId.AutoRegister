namespace SnowflakeId.AutoRegister.MySql.Storage;

public class MySqlQueryProvider : ISqlQueryProvider
{
    public string GetExistQuery(string schemaName) =>
        $"SELECT 1 FROM `{schemaName}_RegisterKeyValues` WHERE `Key` = @key LIMIT 1;";

    public string GetQuery(string schemaName) =>
        $"SELECT `Value` FROM `{schemaName}_RegisterKeyValues` WHERE `Key` = @key;";

    public string GetInsertOrUpdateQuery(string schemaName) =>
        $"""
        INSERT INTO `{schemaName}_RegisterKeyValues` (`Key`, `Value`, `ExpireTime`)
        VALUES (@key, @value, @expireTime)
        ON DUPLICATE KEY UPDATE `Value` = VALUES(`Value`), `ExpireTime` = VALUES(`ExpireTime`);
        """;

    public string GetInsertIfNotExistsQuery(string schemaName) =>
        $"""
        INSERT IGNORE INTO `{schemaName}_RegisterKeyValues` (`Key`, `Value`, `ExpireTime`)
        VALUES (@key, @value, @expireTime);
        """;


    public string GetUpdateExpireQuery(string schemaName) =>
        $"UPDATE `{schemaName}_RegisterKeyValues` SET `ExpireTime` = @expireTime WHERE `Key` = @key AND `Value` = @value;";

    public string GetDeleteQuery(string schemaName) =>
        $"DELETE FROM `{schemaName}_RegisterKeyValues` WHERE `Key` = @key;";

    public string GetClearExpiredQuery(string schemaName) =>
        $"DELETE FROM `{schemaName}_RegisterKeyValues` WHERE `Key` != @key AND `ExpireTime` < @now;";
}