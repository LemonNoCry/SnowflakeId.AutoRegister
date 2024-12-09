namespace SnowflakeId.AutoRegister.StackExchangeRedis.Storage;

/// <summary>
/// Represents a Redis storage mechanism for the SnowflakeId AutoRegister system.
/// </summary>
internal class RedisStorage : IStorage
{
    /// <summary>
    /// Lua script: Checks if the key's value matches the provided value and sets the expiration time
    /// </summary>
    private const string ExpireScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('pexpire', KEYS[1], ARGV[2])
        else
            return 0
        end";

    private readonly IConnectionMultiplexer _connection;
    private readonly RedisStorageOption _storageOption;

    internal RedisStorage(RedisStorageOption storageOption)
    {
        if (storageOption is null)
        {
            throw new ArgumentNullException(nameof(storageOption));
        }

        storageOption.InstanceName ??= RedisStorageOption.DefaultInstanceName;

        _storageOption = storageOption;
        _connection = storageOption.ConnectionMultiplexerFactory?.Invoke() ?? ConnectionMultiplexer.Connect(storageOption.ConfigurationOptions ??
            throw new ArgumentNullException(nameof(storageOption.ConfigurationOptions)));
    }

    public bool Exist(string key)
    {
        var db = _connection.GetDatabase();
        return db.KeyExists(_storageOption.InstanceName + key);
    }

    public string? Get(string key)
    {
        var db = _connection.GetDatabase();
        return db.StringGet(_storageOption.InstanceName + key);
    }

    public bool Set(string key, string value, int millisecond)
    {
        var db = _connection.GetDatabase();
        return db.StringSet(_storageOption.InstanceName + key, value, TimeSpan.FromMilliseconds(millisecond));
    }

    public bool SetNotExists(string key, string value, int millisecond)
    {
        var db = _connection.GetDatabase();
        return db.StringSet(_storageOption.InstanceName + key, value, TimeSpan.FromMilliseconds(millisecond), When.NotExists);
    }

    public bool Expire(string key, string value, int millisecond)
    {
        var db = _connection.GetDatabase();
        var redisKey = _storageOption.InstanceName + key;

        var result = (int)db.ScriptEvaluate(
            ExpireScript,
            [redisKey],
            [value, millisecond]);

        return result == 1;
    }

    public async Task<bool> ExpireAsync(string key, string value, int millisecond)
    {
        var db = _connection.GetDatabase();
        var redisKey = _storageOption.InstanceName + key;

        var result = (int)await db.ScriptEvaluateAsync(
            ExpireScript,
            [redisKey],
            [value, millisecond]);

        return result == 1;
    }

    public bool Delete(string key)
    {
        var db = _connection.GetDatabase();
        return db.KeyDelete(_storageOption.InstanceName + key);
    }


    #region Dispose

    public void Dispose()
    {
        if (_storageOption.ConnectionMultiplexerFactory is null)
        {
            // If the connection was created by me (not by the factory), I will dispose it
            _connection.Dispose();
        }
    }

    #endregion
}