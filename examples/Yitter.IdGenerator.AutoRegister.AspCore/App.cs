// ReSharper disable NullableWarningSuppressionIsUsed

namespace Yitter.IdGenerator.AutoRegister.AspCore;

public class App
{
    public static IConfiguration Configuration { get; set; } = null!;
    public static IServiceProvider Service { get; set; } = null!;
}