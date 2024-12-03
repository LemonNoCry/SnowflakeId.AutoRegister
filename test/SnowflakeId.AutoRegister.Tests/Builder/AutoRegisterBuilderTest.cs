// ReSharper disable NullableWarningSuppressionIsUsed

namespace SnowflakeId.AutoRegister.Tests.Builder;

[TestSubject(typeof(AutoRegisterBuilder))]
public class AutoRegisterBuilderTest
{
    [Fact]
    public void UseDefaultStore_ShouldReturnBuilderInstance()
    {
        // Arrange
        var builder = new AutoRegisterBuilder();

        // Act
        var result = builder.UseDefaultStore();

        // Assert
        Assert.IsType<AutoRegisterBuilder>(result);
    }

    [Fact]
    public void UseStore_ShouldThrowArgumentNullException_WhenStorageIsNull()
    {
        // Arrange
        var builder = new AutoRegisterBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.UseStore(null!));
    }

    [Fact]
    public void SetRegisterOption_ShouldThrowArgumentNullException_WhenActionIsNull()
    {
        // Arrange
        var builder = new AutoRegisterBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.SetRegisterOption(null!));
    }

    [Fact]
    public void SetRegisterFactory_ShouldThrowArgumentNullException_WhenFactoryIsNull()
    {
        // Arrange
        var builder = new AutoRegisterBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.SetRegisterFactory(null!));
    }

    [Fact]
    public void Build_ShouldThrowArgumentNullException_WhenStoreIsNull()
    {
        // Arrange
        var builder = new AutoRegisterBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Build());
    }
}