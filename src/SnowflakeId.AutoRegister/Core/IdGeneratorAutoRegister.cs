namespace SnowflakeId.AutoRegister.Core;

/// <summary>
/// Class for auto-registering ID generators.
/// </summary>
internal class IdGeneratorAutoRegister<T> : DefaultAutoRegister, IAutoRegister<T> where T : class
{
    private readonly object _lock = new();
    private readonly Func<SnowflakeIdConfig, T> _registerBuild;
    private T? _idGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdGeneratorAutoRegister{T}" /> class.
    /// </summary>
    /// <param name="storage">The storage instance.</param>
    /// <param name="registerOption">The registration options.</param>
    /// <param name="registerBuild">The function to build the ID generator.</param>
    public IdGeneratorAutoRegister(IStorage storage, SnowflakeIdRegisterOption registerOption, Func<SnowflakeIdConfig, T>? registerBuild)
        : base(storage, registerOption)
    {
        _registerBuild = registerBuild ?? throw new ArgumentNullException(nameof(registerBuild));
    }

    public SnowflakeIdConfig? GetSnowflakeIdConfig()
    {
        return SnowflakeIdConfig;
    }

    /// <summary>
    /// Gets the ID generator instance.
    /// </summary>
    /// <returns>The ID generator instance.</returns>
    public T GetIdGenerator()
    {
        lock (_lock)
        {
            if (_idGenerator != null) return _idGenerator;

            _idGenerator = _registerBuild(base.Register());
            return _idGenerator;
        }
    }

    /// <summary>
    /// Unregisters the ID generator.
    /// </summary>
    public void UnRegisterIdGenerator()
    {
        UnRegister();
    }

    /// <summary>
    /// Unregisters the current Snowflake ID.
    /// </summary>
    public override void UnRegister()
    {
        lock (_lock)
        {
            if (_idGenerator is null) return;

            base.UnRegister();
            _idGenerator = null;
        }
    }

    /// <summary>
    /// Resets the ID generator.
    /// </summary>
    protected override void Reset()
    {
        lock (_lock)
        {
            base.Reset();
            _idGenerator = null;
        }
    }

    #region Dispose

    /// <summary>
    /// Disposes the resources used by the instance.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            base.Dispose(disposing);
            UnRegister();
        }
    }

    ~IdGeneratorAutoRegister()
    {
        Dispose(false);
    }

    #endregion
}