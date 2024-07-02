namespace SnowflakeId.AutoRegister.Tests.Configs;

[TestSubject(typeof(SnowflakeIdRegisterOption))]
public class SnowflakeIdRegisterOptionTest
{
    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenMaxLoopCountIsLessThanOne()
    {
        // Arrange
        var option = new SnowflakeIdRegisterOption { MaxLoopCount = 0 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => option.Validate());
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenWorkerIdLifeMillisecondIsLessThanOrEqualToZero()
    {
        // Arrange
        var option = new SnowflakeIdRegisterOption { WorkerIdLifeMillisecond = 0 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => option.Validate());
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenMinWorkerIdIsGreaterThanMaxWorkerId()
    {
        // Arrange
        var option = new SnowflakeIdRegisterOption { MinWorkerId = 51, MaxWorkerId = 50 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => option.Validate());
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenMinWorkerIdIsLessThanZero()
    {
        // Arrange
        var option = new SnowflakeIdRegisterOption { MinWorkerId = -1 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => option.Validate());
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenMaxWorkerIdIsLessThanZero()
    {
        // Arrange
        var option = new SnowflakeIdRegisterOption { MaxWorkerId = -1 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => option.Validate());
    }
}