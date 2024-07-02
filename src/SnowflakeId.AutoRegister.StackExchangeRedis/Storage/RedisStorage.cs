using SnowflakeId.AutoRegister.StackExchangeRedis.Configs;

namespace SnowflakeId.AutoRegister.StackExchangeRedis.Storage;

/// <summary>
/// Represents a Redis storage mechanism for the SnowflakeId AutoRegister system.
/// </summary>
internal class RedisStorage : IStorage
{
    private readonly RedisStorageOption _storageOption;
    private readonly IConnectionMultiplexer _connection;

    internal RedisStorage(RedisStorageOption storageOption)
    {
        if (storageOption is null)
        {
            throw new ArgumentNullException(nameof(storageOption));
        }

        if (storageOption.ConfigurationOptions is null && storageOption.ConnectionMultiplexerFactory is null)
        {
            throw new ArgumentNullException(nameof(storageOption.ConfigurationOptions));
        }

        storageOption.InstanceName ??= RedisStorageOption.DefaultInstanceName;

        _storageOption = storageOption;
        _connection = storageOption.ConnectionMultiplexerFactory?.Invoke() ?? ConnectionMultiplexer.Connect(storageOption.ConfigurationOptions!);
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

    public bool Expire(string key, int millisecond)
    {
        var db = _connection.GetDatabase();
        return db.KeyExpire(_storageOption.InstanceName + key, TimeSpan.FromMilliseconds(millisecond));
    }

    public Task<bool> ExpireAsync(string key, int millisecond)
    {
        var db = _connection.GetDatabase();
        return db.KeyExpireAsync(_storageOption.InstanceName + key, TimeSpan.FromMilliseconds(millisecond));
    }

    public bool Delete(string key)
    {
        var db = _connection.GetDatabase();
        return db.KeyDelete(_storageOption.InstanceName + key);
    }


    #region disponse

    public void Dispose()
    {
        if (_storageOption.ConnectionMultiplexerFactory is null)
        {
            // If the connection was created by me (not by the factory), I will dispose it
            _connection.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_storageOption.ConnectionMultiplexerFactory is null)
        {
            // If the connection was created by me (not by the factory), I will dispose it
            await _connection.DisposeAsync();
        }
    }

    #endregion
}