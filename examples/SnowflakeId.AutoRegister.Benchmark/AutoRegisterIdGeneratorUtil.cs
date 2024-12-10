using SnowflakeId.AutoRegister.Builder;
using SnowflakeId.AutoRegister.Interfaces;
using Yitter.IdGenerator;

namespace SnowflakeId.AutoRegister.Benchmark;

public class AutoRegisterIdGeneratorUtil
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    private static readonly Lazy<IAutoRegister<IIdGenerator>> AutoRegister = new(() =>
    {
        var builder = new AutoRegisterBuilder()
            // Register Option
            // Use the following line to set the identifier.
            // Recommended setting to distinguish multiple applications on a single machine
           .SetExtraIdentifier(Environment.CurrentDirectory)

            // Differentiate between multiple processes
            //.SetExtraIdentifier(Environment.CurrentDirectory + Process.GetCurrentProcess().Id)

            // Set the log output.
            // .SetLogMinimumLevel(LogLevel.Debug)
            // .SetLogger((level, message, ex) => Console.WriteLine($"[{DateTime.Now}] [{level}] {message} {ex}"))

            // Use the following line to set the WorkerId scope.
           .SetWorkerIdScope(1, 31)
            // Use the following line to set the register option.
            // .SetRegisterOption(option => {})

            // Use the following line to use the default store.
            // Only suitable for standalone use, local testing, etc.
            //.UseDefaultStore()

            // Use the following line to use the Redis store.
           .UseRedisStore("localhost:6379,allowAdmin=true")

            // Use the following line to use the SQL Server store.
            //.UseSqlServerStore("Server=localhost;Database=SnowflakeTest;Integrated Security=SSPI;TrustServerCertificate=true;")

            // Use the following line to use the MySQL store.
            // .UseMySqlStore("Server=localhost;Port=3306;Database=snowflaketest;Uid=test;Pwd=123456;SslMode=None;")

            // Build the IAutoRegister<IIdGenerator> instance.
           .Build<IIdGenerator>(config => new DefaultIdGenerator(new IdGeneratorOptions
            {
                WorkerId = (ushort)config.WorkerId
            }));

        // Unregister on process exit
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            // Unregister the SnowflakeIdConfig.
            builder.UnRegisterIdGenerator();
        };
        return builder;
    });

    private static IIdGenerator IdGenInstance => AutoRegister.Value.GetIdGenerator();

    public static long NextId()
    {
        return IdGenInstance.NewLong();
    }
}

public class IdGeneratorUtil
{
    private static readonly Lazy<IIdGenerator> _idGenerator = new(() => new DefaultIdGenerator(new IdGeneratorOptions
    {
        WorkerId = 1
    }));

    private static IIdGenerator IdGenerator => _idGenerator.Value;

    public static long NextId()
    {
        return IdGenerator.NewLong();
    }
}