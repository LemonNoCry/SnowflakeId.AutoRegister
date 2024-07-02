namespace SnowflakeId.AutoRegister.Tests.Util;

[TestSubject(typeof(MacUtil))]
public class MacUtilTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void MacUtil_GetMacAddress()
    {
        testOutputHelper.WriteLine(MacUtil.GetMacAddress());
    }
}