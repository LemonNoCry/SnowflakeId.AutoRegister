namespace SnowflakeId.AutoRegister.Tests.Redis.Examples;

[TestSubject(typeof(RedisStorage))]
public class RedisAutoRegister : TestBaseAutoRegister, IClassFixture<RedisFixture>
{
    public RedisAutoRegister(ITestOutputHelper testOutputHelper, RedisFixture redisFixture)
        : base(testOutputHelper)
    {
        SetRegisterBuild = builder => builder
           .UseRedisStore(option => { option.ConnectionMultiplexerFactory = () => redisFixture.Connection; });
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

    [Fact]
    protected override void Test_WorkerId_Own_Scramble()
    {
        base.Test_WorkerId_Own_Scramble();
    }
}