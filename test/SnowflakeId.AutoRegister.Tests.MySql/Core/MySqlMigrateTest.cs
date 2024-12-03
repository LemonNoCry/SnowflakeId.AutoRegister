namespace SnowflakeId.AutoRegister.Tests.MySql.Core;

[TestSubject(typeof(MySqlMigrate))]
public class MySqlMigrateTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestMigrateScript()
    {
        var migrateScript = MySqlMigrate.GetMigrateScript();

        // Assert
        Assert.NotNull(migrateScript);
        testOutputHelper.WriteLine(migrateScript);
    }
}