﻿using SnowflakeId.AutoRegister.Tests.Base;

namespace SnowflakeId.AutoRegister.Tests.Redis.Examples;

public class RedisAutoRegister : BaseAutoRegister
{
    private static readonly ConnectionMultiplexer _connection = ConnectionMultiplexer.Connect(ConnectionString);


    public RedisAutoRegister()
    {
        SetRegisterBuild = builder => builder
           .UseRedisStore(option => { option.ConnectionMultiplexerFactory = () => _connection; });
    }

    [Fact]
    protected override Task Test_StorageAsync()
    {
        return base.Test_StorageAsync();
    }

    [Fact]
    protected override void Test_RegisterWorker()
    {
        base.Test_RegisterWorker();
    }

    [Fact]
    protected override Task Test_MultipleConcurrentRegistrations()
    {
        return base.Test_MultipleConcurrentRegistrations();
    }

    [Fact]
    protected override void Test_WorkerId_Own()
    {
        base.Test_WorkerId_Own();
    }

    [Fact]
    protected override void Test_WorkerId_Expired()
    {
         base.Test_WorkerId_Expired();
    }
}