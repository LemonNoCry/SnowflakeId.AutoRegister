using System;

namespace SnowflakeId.AutoRegister.Tests.Redis;

public class RedisTestConfig
{
    public const string ConnectionString = "localhost:6379,allowAdmin=true";
}

public class RedisFixture : IDisposable
{
    public ConnectionMultiplexer Connection { get; } = ConnectionMultiplexer.Connect(ConnectionString);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) Connection.Dispose();
    }
}