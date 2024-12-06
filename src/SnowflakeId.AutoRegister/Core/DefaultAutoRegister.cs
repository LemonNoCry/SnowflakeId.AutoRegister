using SnowflakeId.AutoRegister.Util;

namespace SnowflakeId.AutoRegister.Core;

/// <summary>
/// Provides default implementation for automatic registration.
/// </summary>
internal class DefaultAutoRegister : IAutoRegister
{
    internal const string WorkerIdKeyPrefix = "WorkerId:{0}";
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public readonly string Identifier;
    protected readonly SnowflakeIdRegisterOption RegisterOption;
    protected readonly IStorage Storage;
    protected internal ExtendLifeTimeTask? ExtendLifeTimeTask;
    protected SnowflakeIdConfig? SnowflakeIdConfig;

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
        _semaphore.Wait();
        try
        {
            if (SnowflakeIdConfig != null) return SnowflakeIdConfig;

            SnowflakeIdConfig = new SnowflakeIdConfig
            {
                Identifier = Identifier,
                WorkerId = GetValidWorkerId()
            };

            ExtendLifeTimeTask =
                new ExtendLifeTimeTask(TimeSpan.FromMilliseconds(RegisterOption.WorkerIdLifeMillisecond / 3d),
                    cancellationToken => ExtendLifeTimeOperation(SnowflakeIdConfig, cancellationToken));
            ExtendLifeTimeTask.Start();

            LogManager.Instance.LogInfo($"Register WorkerId:{SnowflakeIdConfig.WorkerId}");
            return SnowflakeIdConfig;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// UnRegister the current Snowflake ID.
    /// </summary>
    public virtual void UnRegister()
    {
        _semaphore.Wait();
        try
        {
            if (SnowflakeIdConfig == null) return;

            // First stop the task, then delete the cache
            // Avoid extending the time again in the task after deleting the Cache first
            ExtendLifeTimeTask?.Stop();
            ExtendLifeTimeTask = null;

            Clear(SnowflakeIdConfig);

            LogManager.Instance.LogInfo($"UnRegister WorkerId:{SnowflakeIdConfig.WorkerId}");
            SnowflakeIdConfig = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected internal string WorkerIdFormat(long workerId) => string.Format(WorkerIdKeyPrefix, workerId);

    /// <summary>
    /// Run long time task. Extend the WorkerId life cycle
    /// Every 1/3 of the SnowflakeIdRegisterOption.WorkerIdLifeMillisecond , the Lifetime is renewed (extended) by the server.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="cancellationToken"></param>
    private async Task ExtendLifeTimeOperation(SnowflakeIdConfig config, CancellationToken cancellationToken)
    {
        // To prevent tasks from executing during logout
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // The task has been canceled, preventing further execution
            if (cancellationToken.IsCancellationRequested)
            {
                Clear(config);
                LogManager.Instance.LogInfo($"Extend WorkerId:{config.WorkerId} Identifier:{Identifier} canceled");
                return;
            }

            LogManager.Instance.LogInfo($"Start extend WorkerId:{config.WorkerId} Identifier:{Identifier} ");

            // Extend the life of the WorkerId
            var key = WorkerIdFormat(config.WorkerId);

            var flag = await Storage.ExpireAsync(key, RegisterOption.WorkerIdLifeMillisecond);
            if (!flag)
            {
                LogManager.Instance.LogWarn($"Extend WorkerId:{config.WorkerId} Identifier:{Identifier} failed,The WorkerId has expired");
                // In theory, you shouldn't go here
                // If the WorkerId is not found in the cache, it means that the WorkerId has expired.
                // Try to re-register the WorkerId
                flag = Storage.SetNotExists(key, Identifier, RegisterOption.WorkerIdLifeMillisecond);
                if (!flag)
                {
                    LogManager.Instance.LogFatal($"Extend WorkerId:{config.WorkerId} Identifier:{Identifier
                    } failed,Cannot be renewed and cannot be reset");

                    // TODO Renewal failed, try to re-register the WorkerId
                    return;
                }
            }

            // Extend the life of the Identifier
            Storage.Set(Identifier, config.WorkerId.ToString(), RegisterOption.WorkerIdLifeMillisecond);

            LogManager.Instance.LogInfo($"Extend WorkerId:{config.WorkerId} Identifier:{Identifier} success");
        }
        catch (Exception e)
        {
            LogManager.Instance.LogError($"Extend WorkerId:{config.WorkerId} Identifier:{Identifier} error", e);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Gets the next available worker ID.
    /// </summary>
    /// <returns>The next available worker ID.</returns>
    protected virtual int GetValidWorkerId()
    {
        LogManager.Instance.LogDebug("Prepare to get WorkerId");

        // First. try to get the WorkerId from the cache
        // If the cache exists, return the WorkerId directly
        var workerIdStr = Storage.Get(Identifier);

        if (!string.IsNullOrEmpty(workerIdStr) && int.TryParse(workerIdStr, out var usedWorkerId))
        {
            LogManager.Instance.LogDebug($"Get last WorkerId:{workerIdStr}");
            var key = WorkerIdFormat(usedWorkerId);

            // Valid WorkerId owned by the current process
            var value = Storage.Get(key);
            if (value == Identifier)
            {
                LogManager.Instance.LogDebug("The WorkerId is still valid");
                // Extend the life of the WorkerId
                var flag = Storage.Expire(key, RegisterOption.WorkerIdLifeMillisecond);
                if (flag)
                {
                    LogManager.Instance.LogDebug("Extend WorkerId life success");
                    Storage.Set(Identifier, key, RegisterOption.WorkerIdLifeMillisecond);
                    return usedWorkerId;
                }

                // If the extension fails, the WorkerId is invalid and needs to be re-registered
                flag = Storage.SetNotExists(key, Identifier, RegisterOption.WorkerIdLifeMillisecond);
                if (flag)
                {
                    LogManager.Instance.LogDebug("Re-register WorkerId success");
                    Storage.Set(Identifier, key, RegisterOption.WorkerIdLifeMillisecond);
                    return usedWorkerId;
                }

                LogManager.Instance.LogDebug("Re-register WorkerId failed, try to get a new WorkerId");
            }
            else
            {
                LogManager.Instance.LogDebug("The WorkerId already belongs to another process");
            }

            // Delete the invalid Identifier
            Storage.Delete(Identifier);
        }

        // If the cache does not exist, try to get the valid WorkerId from the storage
        for (var i = 1; i <= RegisterOption.MaxLoopCount; i++)
        {
            for (var workerId = RegisterOption.MinWorkerId; workerId <= RegisterOption.MaxWorkerId; workerId++)
            {
                LogManager.Instance.LogTrace($"Try to get WorkerId:{workerId}");
                var key = WorkerIdFormat(workerId);
                if (Storage.Exist(key))
                {
                    LogManager.Instance.LogTrace($"{key} already exists");
                    continue;
                }

                // Set WorkerId if the key does not exist
                var flag = Storage.SetNotExists(key, Identifier, RegisterOption.WorkerIdLifeMillisecond);
                if (!flag)
                {
                    // If the key already exists, continue to the next loop
                    LogManager.Instance.LogTrace($"{key} Preempted by other processes");
                    continue;
                }

                // Cache the currently used WorkerId for next use
                Storage.Set(Identifier, workerId.ToString(), RegisterOption.WorkerIdLifeMillisecond);

                LogManager.Instance.LogInfo($"Get WorkerId:{workerId}, Identifier:{Identifier}");
                return workerId;
            }

            // Sleep for a while before the next loop
            if (RegisterOption.SleepMillisecondEveryLoop >= 0) Thread.Sleep(RegisterOption.SleepMillisecondEveryLoop);
        }

        throw new InvalidOperationException("No available worker id.");
    }

    /// <summary>
    /// Clear resources for the specified configuration.
    /// </summary>
    /// <param name="config"></param>
    private void Clear(SnowflakeIdConfig? config)
    {
        if (config is null) return;

        var key = WorkerIdFormat(config.WorkerId);
        Storage.Delete(key);
        Storage.Delete(Identifier);
        LogManager.Instance.LogInfo($"Cleaned up resources for WorkerId:{config.WorkerId} Identifier:{Identifier}");
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