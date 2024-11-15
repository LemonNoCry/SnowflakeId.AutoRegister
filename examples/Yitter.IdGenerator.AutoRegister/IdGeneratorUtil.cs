﻿using SnowflakeId.AutoRegister.Builder;
using SnowflakeId.AutoRegister.Interfaces;

namespace Yitter.IdGenerator.AutoRegister;

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
           .SetExtraIdentifier(Environment.CurrentDirectory)
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
            //.UseSqlServerStore("Server=localhost;Database=IdGenerator;User Id=sa;Password=123456;")
           .Build();

        // Unregister on process exit
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            // Unregister the SnowflakeIdConfig.
            builder.UnRegister();
            Console.WriteLine("Unregistered.");
        };
        return builder;
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
}