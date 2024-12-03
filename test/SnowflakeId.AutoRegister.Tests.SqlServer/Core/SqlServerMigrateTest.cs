namespace SnowflakeId.AutoRegister.Tests.SqlServer.Core;

[TestSubject(typeof(SqlServerMigrate))]
public class SqlServerMigrateTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestMigrateScript()
    {
        var migrateScript = ResourceManager.MigrateScript;

        // Assert
        Assert.NotNull(migrateScript);
        testOutputHelper.WriteLine(migrateScript);
    }
}