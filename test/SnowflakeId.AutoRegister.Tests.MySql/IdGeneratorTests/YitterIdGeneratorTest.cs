using SnowflakeId.AutoRegister.Tests.IdGeneratorTests;

namespace SnowflakeId.AutoRegister.Tests.MySql.IdGeneratorTests;

[TestSubject(typeof(MySqlStorage))]
public class YitterIdGeneratorTest : BaseYitterIdGeneratorTest
{
    public YitterIdGeneratorTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        SetRegisterBuild = builder => builder.UseMySqlStore(ConnectionString);
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