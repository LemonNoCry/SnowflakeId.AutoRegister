namespace SnowflakeId.AutoRegister.Tests.Base;

public class TestBaseAutoRegister
{
    protected TestBaseAutoRegister()
    {
        SetRegisterBuild = builder => builder;
    }

    protected Func<AutoRegisterBuilder, AutoRegisterBuilder> SetRegisterBuild { get; set; }

    protected AutoRegisterBuilder GetAutoRegisterBuilder(AutoRegisterBuilder? builder = null)
    {
        builder ??= new AutoRegisterBuilder();
        return SetRegisterBuild(builder);
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

        var expire = storage.Expire(key, millisecond * 10);
        Assert.True(expire);

        expire = await storage.ExpireAsync(key, millisecond * 10);
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

        // dispose all
        foreach (var register in registers)
        {
            register.Dispose();
        }

        // check all keys are expired
        foreach (var task in tasks)
        {
            var flag = storage?.Exist($@"WorkerId:{task.Result.WorkerId}");
            Assert.False(flag);
        }
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
           .SetRegisterOption(option => option.WorkerIdLifeMillisecond = 900)
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

        // Sleep for a while to expire the worker id
        Thread.Sleep(1000);

        // Get the worker id
        workerId = storage?.Get(idConfig.Identifier);
        Assert.NotNull(workerId);

        // Get the identifier from redis
        identifier = storage?.Get("WorkerId:" + idConfig.WorkerId);
        Assert.NotNull(identifier);
    }
}