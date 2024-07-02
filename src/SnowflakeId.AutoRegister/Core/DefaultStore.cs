using System.Collections.Concurrent;

namespace SnowflakeId.AutoRegister.Core;

public class DefaultStore : IStorage
{
    private readonly ConcurrentDictionary<string, string?> _store = new();

    public bool Exist(string key)
    {
        return _store.ContainsKey(key);
    }

    public string? Get(string key)
    {
        return _store.TryGetValue(key, out var value) ? value : default;
    }

    public bool Set(string key, string value, int millisecond)
    {
        _store[key] = value;
        return true;
    }

    public bool SetNotExists(string key, string value, int millisecond)
    {
        return _store.TryAdd(key, value);
    }

    public bool Expire(string key, int millisecond)
    {
        return true;
    }

    public Task<bool> ExpireAsync(string key, int millisecond)
    {
        return Task.FromResult(true);
    }

    public bool Delete(string key)
    {
        return _store.TryRemove(key, out _);
    }

    #region disponse

    public void Dispose()
    {
        _store.Clear();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return default;
    }

    #endregion
}