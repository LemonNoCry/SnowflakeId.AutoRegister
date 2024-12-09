namespace SnowflakeId.AutoRegister.Tests.Base;

[Collection("Non-Parallel Collection")]
[TestSubject(typeof(IStorage))]
[TestSubject(typeof(IAutoRegister))]
[TestSubject(typeof(AutoRegisterBuilder))]
public class TestBaseAutoRegister
{
    private readonly ITestOutputHelper _testOutputHelper;

    protected TestBaseAutoRegister(ITestOutputHelper testOutputHelper)
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

    protected IAutoRegister GetAutoRegister(AutoRegisterBuilder? builder = null)
    {
        return GetAutoRegisterBuilder(builder).Build();
    }

    protected virtual async Task Test_StorageAsync()
    {
        var builder = GetAutoRegisterBuilder()
           .SetExtraIdentifier(this.GetType().FullName + nameof(Test_StorageAsync));

        var storage = builder.Storage;

        Assert.NotNull(storage);

        // full test storage
        var key = "test";
        var value = "test";
        var millisecond = 10000;
        var flag = storage.Set(key, value, millisecond);
        Assert.True(flag);

        flag = storage.SetNotExists(key, value, millisecond);
        Assert.False(flag);

        var exist = storage.Exist(key);
        Assert.True(exist);

        var getValue = storage.Get(key);
        Assert.Equal(value, getValue);

        var expire = storage.Expire(key, value, millisecond * 10);
        Assert.True(expire);

        expire = await storage.ExpireAsync(key, value, millisecond * 10);
        Assert.True(expire);

        var delete = storage.Delete(key);
        Assert.True(delete);

        var notExist = storage.Exist(key);
        Assert.False(notExist);
    }

    protected virtual void Test_RegisterWorker()
    {
        var builder = GetAutoRegisterBuilder()
           .SetExtraIdentifier(this.GetType().FullName + nameof(Test_RegisterWorker));
        using var register = GetAutoRegister(builder);
        var idConfig = register.Register();

        Assert.NotNull(idConfig);
        Assert.NotEqual(0, idConfig.WorkerId);
    }

    protected virtual async Task Test_MultipleConcurrentRegistrations()
    {
        IStorage? storage = null;
        var registers = new IAutoRegister[10];
        var tasks = new Task<SnowflakeIdConfig>[10];

        for (var i = 0; i < tasks.Length; i++)
        {
            var currentIndex = i;
            var builder = GetAutoRegisterBuilder()
               .SetExtraIdentifier(this.GetType().FullName + nameof(Test_MultipleConcurrentRegistrations) + currentIndex);
            storage = builder.Storage;
            var idAutoRegister = builder.Build();

            registers[i] = idAutoRegister;
            tasks[i] = Task.Run(() => idAutoRegister.Register());
        }

        await Task.WhenAll(tasks);

        var workerIds = tasks.Select(x => x.Result.WorkerId).ToArray();
        Assert.Equal(tasks.Length, workerIds.Length);
        // Assert all worker ids are unique
        Assert.Equal(workerIds.Length, workerIds.Distinct().Count());

        foreach (var workerId in workerIds)
        {
            Assert.NotEqual(0, workerId);
        }

        Assert.NotNull(storage);

        // Unregister all
        foreach (var register in registers)
        {
            register.UnRegister();
        }

        foreach (var task in tasks)
        {
            //Wait for the WorkerId to be deleted
            while (storage.Exist(AppConst.WorkerIdFormat(task.Result.WorkerId)))
            {
                _testOutputHelper.WriteLine("[Test_MultipleConcurrentRegistrations] Wait for WorkerId to expire");
                Thread.Sleep(101);
            }
        }

        // check all keys are expired
        foreach (var task in tasks)
        {
            var flag = storage?.Exist($@"WorkerId:{task.Result.WorkerId}");
            Assert.False(flag);
        }

        // dispose all
        foreach (var register in registers) register.Dispose();
    }

    protected virtual void Test_WorkerId_Own()
    {
        var builder = GetAutoRegisterBuilder()
           .SetExtraIdentifier(this.GetType().FullName + nameof(Test_WorkerId_Own));
        var storage = builder.Storage;
        using var register = GetAutoRegister(builder);
        var idConfig = register.Register();

        Assert.NotNull(idConfig);
        Assert.NotEqual(0, idConfig.WorkerId);
        Assert.NotEmpty(idConfig.Identifier);

        // Get the worker id
        var workerId = storage?.Get(idConfig.Identifier);
        Assert.Equal(idConfig.WorkerId.ToString(), workerId);

        // Get the identifier
        var identifier = storage?.Get("WorkerId:" + idConfig.WorkerId);
        Assert.Equal(idConfig.Identifier, identifier);
    }

    protected virtual void Test_WorkerId_Expired()
    {
        var builder = GetAutoRegisterBuilder()
           .SetExtraIdentifier(this.GetType().FullName + nameof(Test_WorkerId_Expired));
        var storage = builder.Storage;
        using var register = builder
           .SetWorkerIdLifeMillisecond(900)
           .Build();
        var idConfig = register.Register();

        Assert.NotNull(idConfig);
        Assert.NotEqual(0, idConfig.WorkerId);
        Assert.NotEmpty(idConfig.Identifier);

        // Get the worker id
        var workerId = storage?.Get(idConfig.Identifier);
        Assert.Equal(idConfig.WorkerId.ToString(), workerId);

        // Get the identifier
        var identifier = storage?.Get("WorkerId:" + idConfig.WorkerId);
        Assert.Equal(idConfig.Identifier, identifier);

        // Sleep for a while, is it expired?
        Thread.Sleep(901);

        // Get the worker id
        workerId = storage?.Get(idConfig.Identifier);
        Assert.NotNull(workerId);

        // Get the identifier from redis
        identifier = storage?.Get("WorkerId:" + idConfig.WorkerId);
        Assert.NotNull(identifier);
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

        using var register = (DefaultAutoRegister)GetAutoRegister(builder);
        var idConfig = register.Register();
        Assert.NotNull(idConfig);
        Assert.NotEqual(0, idConfig.WorkerId);
        Assert.NotEmpty(idConfig.Identifier);

        Assert.True(storage.Exist(AppConst.WorkerIdFormat(idConfig.WorkerId)));
        Assert.True(storage.Exist(idConfig.Identifier));

        //Actively delete key,Simulate fake death problem
        Assert.NotNull(register.ExtendLifeTimeTask);
        register.ExtendLifeTimeTask.Stop();

        while (storage.Exist(AppConst.WorkerIdFormat(idConfig.WorkerId)))
        {
            _testOutputHelper.WriteLine("[Test_WorkerId_Own_Scramble] Wait for WorkerId to expire");
            Thread.Sleep(301);
        }

        var builder2 = GetAutoRegisterBuilder()
           .SetWorkerIdLifeMillisecond(900)
           .SetExtraIdentifier(GetType().FullName + nameof(Test_WorkerId_Own_Scramble) + "2");
        using var register2 = (DefaultAutoRegister)GetAutoRegister(builder2);
        var idConfig2 = register2.Register();

        Assert.NotNull(idConfig2);
        Assert.NotEqual(0, idConfig2.WorkerId);
        Assert.NotEmpty(idConfig2.Identifier);

        //Because process 1 is in a "pseudo-dead state" or other situations where renewal is not possible, process 2 obtains the same WorkId as process 1.
        Assert.Equal(idConfig.WorkerId, idConfig2.WorkerId);
        Assert.NotEqual(idConfig.Identifier, idConfig2.Identifier);

        //Process 1 try to renew the WorkerId
        Assert.NotNull(register.ExtendLifeTimeTask);
        Assert.NotNull(register2.ExtendLifeTimeTask);
        register.ExtendLifeTimeTask.Start();

        while (storage.Exist(idConfig.Identifier))
        {
            _testOutputHelper.WriteLine("Wait for Process 1 to reset");
            Thread.Sleep(100);
        }

        // Because process 1 starts to recover, WorkId is still held by process 2, and process 1 also marks that it also has WorkId
        Assert.False(storage.Exist(idConfig.Identifier));
        Assert.Equal(idConfig2.Identifier, storage.Get(AppConst.WorkerIdFormat(idConfig2.WorkerId)));
    }
}