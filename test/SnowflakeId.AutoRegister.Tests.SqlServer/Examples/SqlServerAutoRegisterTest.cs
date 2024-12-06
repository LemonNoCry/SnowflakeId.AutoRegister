namespace SnowflakeId.AutoRegister.Tests.SqlServer.Examples;

[TestSubject(typeof(SqlServerStorage))]
public class SqlServerAutoRegisterTest : TestBaseAutoRegister
{
    public SqlServerAutoRegisterTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        SetRegisterBuild = builder => builder.UseSqlServerStore(ConnectionString);
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
    protected override async Task Test_MultipleConcurrentRegistrations()
    {
        await base.Test_MultipleConcurrentRegistrations();
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