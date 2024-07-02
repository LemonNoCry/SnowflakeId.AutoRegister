using SnowflakeId.AutoRegister.Builder;
using SnowflakeId.AutoRegister.Interfaces;

namespace Yitter.IdGenerator.AutoRegister.AspCore;

public static class IdGeneratorUtil
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    private static readonly Lazy<IAutoRegister> AutoRegister = new(() =>
    {
        var builder = new AutoRegisterBuilder()
                // Register Option
                // Use the following line to set the identifier.
                // Recommended setting to distinguish multiple applications on a single machine
               .SetExtraIdentifier(App.Configuration["urls"])
                // Use the following line to set the WorkerId scope.
               .SetWorkerIdScope(1, 31)
            // Use the following line to set the register option.
            // .SetRegisterOption(option => {})
            ;

        if (App.Configuration["Redis:Enabled"] == "True")
        {
            // Use the following line to use the Redis store.
            builder.UseRedisStore(App.Configuration["Redis:ConnectionString"]);
        }
        else if (App.Configuration["SqlServer:Enabled"] == "True")
        {
            // Use the following line to use the SQL Server store.
            builder.UseSqlServerStore(App.Configuration["SqlServer:ConnectionString"]);
        }
        else
        {
            // Use the following line to use the default store.
            // Only suitable for standalone use, local testing, etc.
            builder.UseDefaultStore();
        }

        var register = builder.Build();
        return register;
    });

    private static readonly Lazy<IIdGenerator> _idGenInstance = new(() =>
    {
        var config = AutoRegister.Value.Register();
        var options = new IdGeneratorOptions
        {
            WorkerId = (ushort)config.WorkerId,
        };
        IIdGenerator idGenInstance = new DefaultIdGenerator(options);
        return idGenInstance;
    });

    private static IIdGenerator IdGenInstance => _idGenInstance.Value;

    public static long NextId()
    {
        return IdGenInstance.NewLong();
    }

    public static void UnRegister()
    {
        if (AutoRegister.IsValueCreated)
        {
            AutoRegister.Value.UnRegister();
            Console.WriteLine("UnRegister");
        }
    }
}