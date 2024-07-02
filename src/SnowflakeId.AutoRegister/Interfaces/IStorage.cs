namespace SnowflakeId.AutoRegister.Interfaces;

/// <summary>
/// Defines a storage mechanism for the SnowflakeId AutoRegister system.
/// </summary>
public interface IStorage : IDisposable, IAsyncDisposable
{
    bool Exist(string key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key, if the key is found; otherwise, <see langword="null"/>.</returns>
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
    /// Sets the time to live in milliseconds of the specified key.
    /// </summary>
    /// <param name="key">The key of the value to expire.</param>
    /// <param name="millisecond">The life of the value in milliseconds.</param>
    bool Expire(string key, int millisecond);

    /// <summary>
    /// Sets the time to live in milliseconds of the specified key.
    /// </summary>
    /// <param name="key">The key of the value to expire.</param>
    /// <param name="millisecond">The life of the value in milliseconds.</param>
    Task<bool> ExpireAsync(string key, int millisecond);

    /// <summary>
    /// Deletes the specified key.
    /// </summary>
    /// <param name="key">The key to delete.</param>
    /// <returns><see langword="true"/> if the key was deleted; otherwise, <see langword="false"/>.</returns>
    bool Delete(string key);
}