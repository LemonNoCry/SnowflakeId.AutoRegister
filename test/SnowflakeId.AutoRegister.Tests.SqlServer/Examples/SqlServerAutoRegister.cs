using SnowflakeId.AutoRegister.Tests.Base;

namespace SnowflakeId.AutoRegister.Tests.SqlServer.Examples;

public class SqlServerAutoRegister : BaseAutoRegister
{
    public SqlServerAutoRegister()
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
}