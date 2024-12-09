namespace SnowflakeId.AutoRegister.Builder;

/// <summary>
/// Builder for configuring and creating an instance of IAutoRegister.
/// </summary>
public class AutoRegisterBuilder
{
    private readonly SnowflakeIdRegisterOption _registerOption = new();

    private Func<IStorage, SnowflakeIdRegisterOption, IAutoRegister> _registerFactory = (storage, option) =>
        new DefaultAutoRegister(storage, option);

    private IStorage? _store;

    #region internal

    internal IStorage? Storage => _store;

    #endregion

    #region RegisterFactory

    /// <summary>
    /// Sets the factory method for creating an IAutoRegister instance.
    /// </summary>
    /// <param name="factory">The factory method for creating an IAutoRegister instance.</param>
    /// <returns>The AutoRegisterBuilder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the factory parameter is null.</exception>
    public AutoRegisterBuilder SetRegisterFactory(Func<IStorage, SnowflakeIdRegisterOption, IAutoRegister> factory)
    {
        _registerFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    #endregion

    /// <summary>
    /// Builds an instance of IAutoRegister using the configured options and storage.
    /// </summary>
    /// <returns>An instance of IAutoRegister.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the storage is not set.</exception>
    public IAutoRegister Build()
    {
        if (_store is null) throw new ArgumentNullException(nameof(_store), "Please use UseDefaultStore or UseStore method to set the storage");

        _registerOption.Validate();

        return _registerFactory(_store, _registerOption);
    }

    /// <summary>
    /// Build a SnowflakeId generator.
    /// </summary>
    /// <param name="registerBuild">A function to build the SnowflakeId generator.</param>
    /// <typeparam name="T">The type of the SnowflakeId generator.</typeparam>
    /// <returns>An instance of IAutoRegister&lt;T&gt;.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the storage or registerBuild function is null.</exception>
    public IAutoRegister<T> Build<T>(Func<SnowflakeIdConfig, T> registerBuild) where T : class
    {
        if (_store is null) throw new ArgumentNullException(nameof(_store), "Please use UseDefaultStore or UseStore method to set the storage");
        if (registerBuild is null) throw new ArgumentNullException(nameof(registerBuild), "Please provide a registerBuild function");

        _registerOption.Validate();

        return new IdGeneratorAutoRegister<T>(_store, _registerOption, registerBuild);
    }

    #region Storage

    /// <summary>
    /// Use default storage<br/>
    /// <para>Suitable for standalone use, local testing, etc</para>
    /// </summary>
    /// <returns></returns>
    public AutoRegisterBuilder UseDefaultStore()
    {
        _store = new DefaultStore();
        return this;
    }

    /// <summary>
    /// Use custom storage
    /// </summary>
    /// <param name="storage">custom storage</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">storage is null</exception>
    public AutoRegisterBuilder UseStore(IStorage storage)
    {
        _store = storage ?? throw new ArgumentNullException(nameof(storage));
        return this;
    }

    #endregion

    #region RegisterOption

    /// <summary>
    /// Sets the options for registering a SnowflakeId.
    /// </summary>
    /// <param name="action">An action to configure the SnowflakeIdRegisterOption.</param>
    /// <returns>The AutoRegisterBuilder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the action parameter is null.</exception>
    public AutoRegisterBuilder SetRegisterOption(Action<SnowflakeIdRegisterOption> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        action(_registerOption);
        return this;
    }

    /// <summary>
    /// Sets the extra identifier for the SnowflakeId.<br/>
    /// Recommended setting to distinguish multiple applications on a single machine
    /// </summary>
    /// <param name="extraIdentifier">The extra identifier for the SnowflakeId.</param>
    /// <returns>The AutoRegisterBuilder instance.</returns>
    public AutoRegisterBuilder SetExtraIdentifier(string extraIdentifier)
    {
        _registerOption.ExtraIdentifier = extraIdentifier;
        return this;
    }

    /// <summary>
    /// Sets the range of WorkerId that can be used by the SnowflakeId generator.
    /// </summary>
    /// <param name="minWorkerId">The minimum WorkerId that can be used. Must be greater than or equal to 0.</param>
    /// <param name="maxWorkerId">The maximum WorkerId that can be used. Must be greater than or equal to 0 and should be greater than minWorkerId.</param>
    /// <returns>The AutoRegisterBuilder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when minWorkerId is less than 0, maxWorkerId is less than 0, or minWorkerId is greater than maxWorkerId.</exception>
    public AutoRegisterBuilder SetWorkerIdScope(int minWorkerId, int maxWorkerId)
    {
        // Validate
        if (minWorkerId < 0)
        {
            throw new ArgumentException("MinWorkerId must be greater than or equal to 0");
        }

        if (maxWorkerId < 0)
        {
            throw new ArgumentException("MaxWorkerId must be greater than or equal to 0");
        }

        if (minWorkerId > maxWorkerId)
        {
            throw new ArgumentException("MinWorkerId must be less than or equal to MaxWorkerId");
        }

        _registerOption.MinWorkerId = minWorkerId;
        _registerOption.MaxWorkerId = maxWorkerId;
        return this;
    }

    /// <summary>
    /// Gets or sets the lifetime of a WorkerId in milliseconds. The default value is 30000 (30 seconds).<br />
    /// Don't set it too low, otherwise, the WorkerId may be released before the WorkerId is actually expired.
    /// </summary>
    /// <param name="workerIdLifeMillisecond">ms</param>
    /// <returns></returns>
    public AutoRegisterBuilder SetWorkerIdLifeMillisecond(int workerIdLifeMillisecond)
    {
        _registerOption.WorkerIdLifeMillisecond = workerIdLifeMillisecond;
        return this;
    }

    #endregion

    #region Logging

    /// <summary>
    /// Sets the logger for the AutoRegister system.
    /// </summary>
    /// <param name="logAction"></param>
    /// <returns></returns>
    public AutoRegisterBuilder SetLogger(Action<LogLevel, string, Exception?>? logAction)
    {
        LogManager.Instance.SetLogAction(logAction);
        return this;
    }

    /// <summary>
    /// Sets the minimum log level for the logger.
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public AutoRegisterBuilder SetLogMinimumLevel(LogLevel logLevel)
    {
        LogManager.Instance.SetMinimumLogLevel(logLevel);
        return this;
    }

    #endregion
}