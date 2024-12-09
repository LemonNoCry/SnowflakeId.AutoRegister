namespace SnowflakeId.AutoRegister.Interfaces;

/// <summary>
/// Defines a storage mechanism for the SnowflakeId AutoRegister system.
/// </summary>
public interface IStorage : IDisposable
{
    /// <summary>
    /// Checks if the specified key exists in the storage.
    /// </summary>
    bool Exist(string key);

    /// <summary>
    /// Retrieves the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose associated value is to be retrieved.</param>
    /// <returns>The value associated with the specified key, or <see langword="null"/> if the key is not found.</returns>
    string? Get(string key);

    /// <summary>
    /// Sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to set.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="millisecond">The life of the value in milliseconds.</param>
    bool Set(string key, string value, int millisecond);

    /// <summary>
    /// Sets the value associated with the specified key if the key does not exist.
    /// </summary>
    /// <param name="key">The key of the value to set.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="millisecond">The life of the value in milliseconds.</param>
    bool SetNotExists(string key, string value, int millisecond);

    /// <summary>
    /// Sets the time to live in milliseconds for the specified key.
    /// </summary>
    /// <param name="key">The key of the value to expire.</param>
    /// <param name="value">The value associated with the key.</param>
    /// <param name="millisecond">The life of the value in milliseconds.</param>
    bool Expire(string key, string value, int millisecond);

    /// <summary>
    /// Sets the time to live in milliseconds for the specified key asynchronously.
    /// </summary>
    /// <param name="key">The key of the value to expire.</param>
    /// <param name="value">The value associated with the key.</param>
    /// <param name="millisecond">The life of the value in milliseconds.</param>
    Task<bool> ExpireAsync(string key, string value, int millisecond);

    /// <summary>
    /// Deletes the specified key.
    /// </summary>
    /// <param name="key">The key to delete.</param>
    /// <returns><see langword="true"/> if the key was deleted; otherwise, <see langword="false"/>.</returns>
    bool Delete(string key);
}