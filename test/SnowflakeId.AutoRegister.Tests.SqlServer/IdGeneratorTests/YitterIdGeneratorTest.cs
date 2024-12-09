using SnowflakeId.AutoRegister.Tests.IdGeneratorTests;

namespace SnowflakeId.AutoRegister.Tests.SqlServer.IdGeneratorTests;

[TestSubject(typeof(SqlServerStorage))]
public class YitterIdGeneratorTest : BaseYitterIdGeneratorTest, IClassFixture<SqlServerFixture>
{
    public YitterIdGeneratorTest(ITestOutputHelper testOutputHelper, SqlServerFixture sqlServerFixture) : base(testOutputHelper)
    {
        SetRegisterBuild = builder => builder.UseSqlServerStore(ConnectionString);
        testOutputHelper.WriteLine("Count: " + sqlServerFixture.GetCount());
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