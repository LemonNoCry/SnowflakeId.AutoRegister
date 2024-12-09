using Yitter.IdGenerator;

namespace SnowflakeId.AutoRegister.Tests.IdGeneratorTests;

[Collection("Non-Parallel Collection")]
[TestSubject(typeof(IAutoRegister<>))]
public class BaseYitterIdGeneratorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    protected BaseYitterIdGeneratorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        SetRegisterBuild = builder => builder;
        LogAction = testOutputHelper.GetLogAction();
        _testOutputHelper.WriteLine($"Current Test: [{Environment.CurrentManagedThreadId}]");
    }

    protected Func<AutoRegisterBuilder, AutoRegisterBuilder> SetRegisterBuild { get; set; }
    protected Action<LogLevel, string, Exception?>? LogAction { get; set; }

    protected AutoRegisterBuilder GetAutoRegisterBuilder(AutoRegisterBuilder? builder = null)
    {
        builder ??= new AutoRegisterBuilder();
        return SetRegisterBuild(builder)
           .SetLogger(LogAction)
           .SetLogMinimumLevel(LogLevel.Trace);
    }

    protected IAutoRegister<IIdGenerator> GetAutoRegister(AutoRegisterBuilder? builder = null)
    {
        return GetAutoRegisterBuilder(builder)
           .Build<IIdGenerator>(config => new DefaultIdGenerator(new IdGeneratorOptions
            {
                WorkerId = (ushort)config.WorkerId
            }));
    }

    protected virtual void Test_Register()
    {
        var builder = GetAutoRegisterBuilder()
           .SetExtraIdentifier(GetType().FullName + nameof(Test_Register));
        var storage = builder.Storage;
        Assert.NotNull(storage);

        using var autoRegister = GetAutoRegister(builder);

        var idGenerator = autoRegister.GetIdGenerator();
        Assert.NotNull(idGenerator);

        Assert.True(autoRegister.GetIdGenerator().NewLong() > 0);

        var idConfig = autoRegister.GetSnowflakeIdConfig();
        Assert.NotNull(idConfig);

        // Get the worker id
        var workerId = storage.Get(idConfig.Identifier);
        Assert.Equal(idConfig.WorkerId.ToString(), workerId);

        // Get the identifier
        var identifier = storage.Get("WorkerId:" + idConfig.WorkerId);
        Assert.Equal(idConfig.Identifier, identifier);
    }

    protected virtual void Test_MultipleRegister()
    {
        var builder = GetAutoRegisterBuilder()
           .SetExtraIdentifier(GetType().FullName + nameof(Test_MultipleRegister));
        var storage = builder.Storage;
        Assert.NotNull(storage);

        using var autoRegister = GetAutoRegister(builder);

        var idGenerator = autoRegister.GetIdGenerator();
        Assert.NotNull(idGenerator);

        var idGenerator2 = autoRegister.GetIdGenerator();
        Assert.NotNull(idGenerator2);

        Assert.Same(idGenerator, idGenerator2);
        Assert.True(autoRegister.GetIdGenerator().NewLong() > 0);

        var idConfig = autoRegister.GetSnowflakeIdConfig();
        Assert.NotNull(idConfig);

        // Get the worker id
        var workerId = storage.Get(idConfig.Identifier);
        Assert.Equal(idConfig.WorkerId.ToString(), workerId);

        // Get the identifier
        var identifier = storage.Get("WorkerId:" + idConfig.WorkerId);
        Assert.Equal(idConfig.Identifier, identifier);
    }

    protected virtual async Task Test_MultipleConcurrentRegistrations()
    {
        IStorage? storage = null;
        var registers = new IAutoRegister<IIdGenerator>[5];
        var tasks = new Task<SnowflakeIdConfig>[5];

        for (var i = 0; i < 5; i++)
        {
            var builder = GetAutoRegisterBuilder()
               .SetExtraIdentifier(GetType().FullName + nameof(Test_MultipleConcurrentRegistrations) + i);
            storage ??= builder.Storage;

            var idAutoRegister = GetAutoRegister(builder);

            registers[i] = idAutoRegister;
            tasks[i] = Task.Run(() =>
            {
                idAutoRegister.GetIdGenerator();
                return idAutoRegister.GetSnowflakeIdConfig() ?? throw new ArgumentNullException(nameof(idAutoRegister.GetSnowflakeIdConfig));
            });
        }

        await Task.WhenAll(tasks);

        foreach (var task in tasks)
        {
            Assert.Null(task.Exception);
            Assert.NotNull(task.Result);
        }

        var workerIds = tasks.Select(x => x.Result.WorkerId).ToArray();
        Assert.Equal(tasks.Length, workerIds.Length);
        // Assert all worker ids are unique
        Assert.Equal(workerIds.Length, workerIds.Distinct().Count());

        foreach (var workerId in workerIds) Assert.NotEqual(0, workerId);

        Assert.NotNull(storage);

        // Unregister all
        foreach (var register in registers)
        {
            register.UnRegisterIdGenerator();

            //Wait for the WorkerId to be deleted
            while (storage.Exist(AppConst.WorkerIdFormat(register.GetSnowflakeIdConfig()?.WorkerId ?? 0)))
            {
                _testOutputHelper.WriteLine("[Test_MultipleConcurrentRegistrations] Wait for WorkerId to expire");
                Thread.Sleep(101);
            }
        }

        // check all keys are expired
        foreach (var task in tasks)
        {
            var flag = storage.Exist($@"WorkerId:{task.Result.WorkerId}");
            Assert.False(flag);
        }

        // dispose all
        foreach (var register in registers) register.Dispose();
    }

    /// <summary>
    /// https://github.com/LemonNoCry/SnowflakeId.AutoRegister/issues/2
    /// </summary>
    protected virtual void Test_WorkerId_Own_Scramble()
    {
        var builder = GetAutoRegisterBuilder()
           .SetWorkerIdLifeMillisecond(900)
           .SetExtraIdentifier(GetType().FullName + nameof(Test_WorkerId_Own_Scramble) + "1");
        var storage = builder.Storage;
        Assert.NotNull(storage);

        _testOutputHelper.WriteLine("WorkId Value: " + storage.Get(AppConst.WorkerIdFormat(1)));

        using var register = GetAutoRegister(builder);
        var idGenerator = register.GetIdGenerator();
        Assert.NotNull(idGenerator);

        var idConfig = register.GetSnowflakeIdConfig();
        Assert.NotNull(idConfig);
        Assert.Equal(1, idConfig.WorkerId);

        //Actively delete key,Simulate fake death problem
        var dar = register as DefaultAutoRegister;
        Assert.NotNull(dar);
        Assert.NotNull(dar.ExtendLifeTimeTask);
        dar.ExtendLifeTimeTask.Stop();

        while (storage.Exist(AppConst.WorkerIdFormat(idConfig.WorkerId)))
        {
            _testOutputHelper.WriteLine("[BaseYitterIdGeneratorTest.Test_WorkerId_Own_Scramble] Wait for WorkerId to expire");
            Thread.Sleep(301);
        }

        var builder2 = GetAutoRegisterBuilder()
           .SetWorkerIdLifeMillisecond(900)
           .SetExtraIdentifier(GetType().FullName + nameof(Test_WorkerId_Own_Scramble) + "2");

        using var register2 = GetAutoRegister(builder2);
        var idGenerator2 = register2.GetIdGenerator();
        Assert.NotNull(idGenerator2);

        var idConfig2 = register2.GetSnowflakeIdConfig();
        Assert.NotNull(idConfig2);

        //Because process 1 is in a "pseudo-dead state" or other situations where renewal is not possible, process 2 obtains the same WorkId as process 1.
        Assert.Equal(idConfig.WorkerId, idConfig2.WorkerId);
        Assert.NotEqual(idConfig.Identifier, idConfig2.Identifier);

        // Process 1 recovers the life cycle of the WorkerId.
        // Because process 2 has the WorkerId, process 1 will regenerate its WorkerId.
        dar.ExtendLifeTimeTask.Start();

        while (register.GetSnowflakeIdConfig() != null)
        {
            _testOutputHelper.WriteLine("Wait for Process 1 to reset");
            Thread.Sleep(101);
        }

        var idGenerator3 = register.GetIdGenerator();
        Assert.NotNull(idGenerator3);
        Assert.NotSame(idGenerator, idGenerator3);

        var idConfig3 = register.GetSnowflakeIdConfig();
        Assert.NotNull(idConfig3);
        Assert.Equal(idConfig.Identifier, idConfig3.Identifier);
        Assert.NotEqual(idConfig.WorkerId, idConfig3.WorkerId);

        //Check Store
        Assert.True(storage.Exist(idConfig.Identifier));
        Assert.True(storage.Exist(idConfig2.Identifier));
        Assert.True(storage.Exist(AppConst.WorkerIdFormat(idConfig2.WorkerId)));
        Assert.True(storage.Exist(AppConst.WorkerIdFormat(idConfig3.WorkerId)));
        Assert.Equal(idConfig2.Identifier, storage.Get(AppConst.WorkerIdFormat(idConfig2.WorkerId)));
        Assert.Equal(idConfig3.Identifier, storage.Get(AppConst.WorkerIdFormat(idConfig3.WorkerId)));
    }
}