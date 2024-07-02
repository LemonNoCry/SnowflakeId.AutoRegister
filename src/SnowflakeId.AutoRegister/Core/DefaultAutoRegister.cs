using SnowflakeId.AutoRegister.Util;

namespace SnowflakeId.AutoRegister.Core;

/// <summary>
/// Provides default implementation for automatic registration.
/// </summary>
internal class DefaultAutoRegister : IAutoRegister
{
    internal const string WorkerIdKeyPrefix = "WorkerId:{0}";
    protected readonly IStorage Storage;
    protected readonly SnowflakeIdRegisterOption RegisterOption;
    public readonly string Identifier;
    protected SnowflakeIdConfig? SnowflakeIdConfig;
    protected ExtendLifeTimeTask? ExtendLifeTimeTask;

    protected internal string WorkerIdFormat(long workerId) => string.Format(WorkerIdKeyPrefix, workerId);

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAutoRegister"/> class.
    /// </summary>
    /// <param name="storage">The storage to use.</param>
    /// <param name="registerOption">The registration options to use.</param>
    public DefaultAutoRegister(IStorage storage, SnowflakeIdRegisterOption registerOption)
    {
        Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        RegisterOption = registerOption ?? throw new ArgumentNullException(nameof(registerOption));

        Identifier = Md5Util.ComputeMd5(MacUtil.GetMacAddress() + registerOption.ExtraIdentifier);
    }

    /// <summary>
    /// Registers a new Snowflake ID.
    /// </summary>
    /// <returns>The registered Snowflake ID configuration.</returns>
    public virtual SnowflakeIdConfig Register()
    {
        lock (WorkerIdKeyPrefix)
        {
            if (SnowflakeIdConfig != null)
            {
                return SnowflakeIdConfig;
            }

            SnowflakeIdConfig = new SnowflakeIdConfig
            {
                Identifier = Identifier,
                WorkerId = GetValidWorkerId()
            };

            ExtendLifeTimeTask =
                new ExtendLifeTimeTask(TimeSpan.FromMilliseconds(RegisterOption.WorkerIdLifeMillisecond / 3d), ExtendLifeTimeOperation);
            ExtendLifeTimeTask.Start();

            return SnowflakeIdConfig;

            // Run long time task. Extend the WorkerId life cycle
            // Every 1/3 of the SnowflakeIdRegisterOption.WorkerIdLifeMillisecond , the Lifetime is renewed (extended) by the server.
            async Task ExtendLifeTimeOperation(CancellationToken cancellationToken)
            {
                // The task has been canceled, preventing further execution
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                // Extend the life of the WorkerId
                var key = WorkerIdFormat(SnowflakeIdConfig.WorkerId);
                var flag = await Storage.ExpireAsync(key, RegisterOption.WorkerIdLifeMillisecond);
                if (!flag && !cancellationToken.IsCancellationRequested)
                {
                    // In theory, you shouldn't go here
                    // If the WorkerId is not found in the cache, it means that the WorkerId has expired.
                    // Try to re-register the WorkerId
                    Storage.SetNotExists(key, Identifier, RegisterOption.WorkerIdLifeMillisecond);
                }

                // The task has been canceled, preventing further execution
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                // Extend the life of the Identifier
                flag = await Storage.ExpireAsync(Identifier, RegisterOption.WorkerIdLifeMillisecond);
                if (!flag && !cancellationToken.IsCancellationRequested)
                {
                    // In theory, you shouldn't go here
                    // If the Identifier is not found in the cache, it means that the Identifier has expired.
                    // Try to re-register the Identifier
                    Storage.SetNotExists(Identifier, SnowflakeIdConfig.WorkerId.ToString(), RegisterOption.WorkerIdLifeMillisecond);
                }
            }
        }
    }

    /// <summary>
    /// UnRegister the current Snowflake ID.
    /// </summary>
    public virtual void UnRegister()
    {
        lock (WorkerIdKeyPrefix)
        {
            if (SnowflakeIdConfig == null)
            {
                return;
            }

            Storage.Delete(WorkerIdFormat(SnowflakeIdConfig.WorkerId));
            Storage.Delete(Identifier);

            SnowflakeIdConfig = null;
            ExtendLifeTimeTask?.Stop();
        }
    }

    /// <summary>
    /// Gets the next available worker ID.
    /// </summary>
    /// <returns>The next available worker ID.</returns>
    protected virtual int GetValidWorkerId()
    {
        lock (WorkerIdKeyPrefix)
        {
            // First. try to get the WorkerId from the cache
            // If the cache exists, return the WorkerId directly
            var workerIdStr = Storage.Get(Identifier);
            if (!string.IsNullOrEmpty(workerIdStr) && int.TryParse(workerIdStr, out var usedWorkerId))
            {
                var key = WorkerIdFormat(usedWorkerId);

                // Valid WorkerId owned by the current process
                var value = Storage.Get(key);
                if (value == Identifier)
                {
                    // Extend the life of the WorkerId
                    Storage.Expire(key, RegisterOption.WorkerIdLifeMillisecond);
                    Storage.Expire(Identifier, RegisterOption.WorkerIdLifeMillisecond);
                    return usedWorkerId;
                }
            }

            // If the cache does not exist, try to get the valid WorkerId from the storage
            for (var i = 1; i <= RegisterOption.MaxLoopCount; i++)
            {
                for (var workerId = RegisterOption.MinWorkerId; workerId <= RegisterOption.MaxWorkerId; workerId++)
                {
                    var key = WorkerIdFormat(workerId);
                    if (Storage.Exist(key))
                    {
                        continue;
                    }

                    // Set WorkerId if the key does not exist
                    var flag = Storage.SetNotExists(key, Identifier, RegisterOption.WorkerIdLifeMillisecond);
                    if (!flag)
                    {
                        // If the key already exists, continue to the next loop
                        continue;
                    }

                    // Cache the currently used WorkerId for next use
                    Storage.Set(Identifier, workerId.ToString(), RegisterOption.WorkerIdLifeMillisecond);
                    return workerId;
                }

                // Sleep for a while before the next loop
                if (RegisterOption.SleepMillisecondEveryLoop >= 0)
                {
                    Thread.Sleep(RegisterOption.SleepMillisecondEveryLoop);
                }
            }

            throw new InvalidOperationException("No available worker id.");
        }
    }

    #region disponse

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            UnRegister();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}