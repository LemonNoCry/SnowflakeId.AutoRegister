namespace SnowflakeId.AutoRegister.Builder;

/// <summary>
/// Builder for configuring and creating an instance of IAutoRegister.
/// </summary>
public class AutoRegisterBuilder
{
    private IStorage? _store;
    private readonly SnowflakeIdRegisterOption _registerOption = new();

    private Func<IStorage, SnowflakeIdRegisterOption, IAutoRegister> _registerFactory = (storage, option) =>
        new DefaultAutoRegister(storage, option);

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

    #region internal

    internal IStorage? Storage => _store;

    #endregion

    public IAutoRegister Build()
    {
        if (_store is null)
        {
            throw new ArgumentNullException(nameof(_store), "Please use UseDefaultStore or UseStore method to set the storage");
        }

        _registerOption.Validate();

        return _registerFactory(_store, _registerOption);
    }
}