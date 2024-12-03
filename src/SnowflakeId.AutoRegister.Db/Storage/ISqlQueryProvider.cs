namespace SnowflakeId.AutoRegister.Db.Storage;

/// <summary>
/// Provides SQL query strings for various operations in the SnowflakeId AutoRegister system.
/// </summary>
public interface ISqlQueryProvider
{
    /// <summary>
    /// Gets the SQL query string to check if a key exists.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>The SQL query string.</returns>
    string GetExistQuery(string schemaName);

    /// <summary>
    /// Gets the SQL query string to retrieve a value by key.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>The SQL query string.</returns>
    string GetQuery(string schemaName);

    /// <summary>
    /// Gets the SQL query string to insert or update a key-value pair.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>The SQL query string.</returns>
    string GetInsertOrUpdateQuery(string schemaName);

    /// <summary>
    /// Gets the SQL query string to insert a key-value pair if the key does not exist.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>The SQL query string.</returns>
    string GetInsertIfNotExistsQuery(string schemaName);

    /// <summary>
    /// Gets the SQL query string to update the expiration time of a key.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>The SQL query string.</returns>
    string GetUpdateExpireQuery(string schemaName);

    /// <summary>
    /// Gets the SQL query string to delete a key-value pair.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>The SQL query string.</returns>
    string GetDeleteQuery(string schemaName);

    /// <summary>
    /// Gets the SQL query string to clear expired key-value pairs.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>The SQL query string.</returns>
    string GetClearExpiredQuery(string schemaName);
}