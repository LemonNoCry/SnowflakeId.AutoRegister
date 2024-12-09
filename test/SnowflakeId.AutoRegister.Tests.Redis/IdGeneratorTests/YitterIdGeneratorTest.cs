using SnowflakeId.AutoRegister.Tests.IdGeneratorTests;

namespace SnowflakeId.AutoRegister.Tests.Redis.IdGeneratorTests;

[TestSubject(typeof(RedisStorage))]
public class YitterIdGeneratorTest : BaseYitterIdGeneratorTest, IClassFixture<RedisFixture>
{
    public YitterIdGeneratorTest(ITestOutputHelper testOutputHelper, RedisFixture redisFixture) : base(testOutputHelper)
    {
        SetRegisterBuild = builder => builder.UseRedisStore(option => { option.ConnectionMultiplexerFactory = () => redisFixture.Connection; });
    }

    [Fact]
    protected override void Test_Register()
    {
        base.Test_Register();
    }

    [Fact]
    protected override void Test_MultipleRegister()
    {
        base.Test_MultipleRegister();
    }

    [Fact]
    protected override Task Test_MultipleConcurrentRegistrations()
    {
        return base.Test_MultipleConcurrentRegistrations();
    }

    [Fact]
    protected override void Test_WorkerId_Own_Scramble()
    {
        base.Test_WorkerId_Own_Scramble();
    }
}