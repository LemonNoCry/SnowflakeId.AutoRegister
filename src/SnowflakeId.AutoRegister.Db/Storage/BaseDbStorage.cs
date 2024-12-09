namespace SnowflakeId.AutoRegister.Db.Storage;

/// <summary>
/// Represents a base class for database storage mechanisms for the SnowflakeId AutoRegister system.
/// </summary>
public abstract class BaseDbStorage : IStorage
{
    protected readonly Func<DbConnection> ConnectionFactory;
    protected readonly BaseDbStorageOptions Options;
    protected readonly ISqlQueryProvider SqlQueryProvider;

    protected BaseDbStorage(BaseDbStorageOptions options, ISqlQueryProvider sqlQueryProvider)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Options.Validate();
        SqlQueryProvider = sqlQueryProvider;

        ConnectionFactory = Options.ConnectionFactory ?? DefaultConnectionFactory;
    }

    #region Dispose

    public void Dispose()
    {
        // don't have to dispose the connection
    }

    #endregion

    #region IStorage

    private void ClearExpiredValues(string? key = default)
    {
        UseConnection(connection =>
            connection.Execute(SqlQueryProvider.GetClearExpiredQuery(Options.SchemaName), new { key = key ?? string.Empty, now = DateTime.Now }));
    }

    public bool Exist(string key)
    {
        ClearExpiredValues();
        return UseConnection(connection => connection.ExecuteScalar<int>(SqlQueryProvider.GetExistQuery(Options.SchemaName), new { key }) > 0);
    }

    public string? Get(string key)
    {
        ClearExpiredValues();
        return UseConnection(connection => connection.ExecuteScalar<string?>(SqlQueryProvider.GetQuery(Options.SchemaName), new { key }));
    }

    public bool Set(string key, string value, int millisecond)
    {
        ClearExpiredValues();
        return UseTransaction((connection, transaction) =>
        {
            var parameters = new
            {
                key,
                value,
                expireTime = DateTime.Now.AddMilliseconds(millisecond)
            };

            return connection.Execute(SqlQueryProvider.GetInsertOrUpdateQuery(Options.SchemaName), parameters, transaction) > 0;
        });
    }

    public bool SetNotExists(string key, string value, int millisecond)
    {
        return UseTransaction((connection, transaction) =>
        {
            var parameters = new
            {
                key,
                value,
                expireTime = DateTime.Now.AddMilliseconds(millisecond)
            };

            return connection.Execute(SqlQueryProvider.GetInsertIfNotExistsQuery(Options.SchemaName), parameters, transaction) > 0;
        });
    }

    public bool Expire(string key, string value, int millisecond)
    {
        return UseConnection((connection) =>
        {
            var parameters = new
            {
                key, value,
                expireTime = DateTime.Now.AddMilliseconds(millisecond)
            };

            return connection.Execute(SqlQueryProvider.GetUpdateExpireQuery(Options.SchemaName), parameters) > 0;
        });
    }

    public Task<bool> ExpireAsync(string key, string value, int millisecond)
    {
        return UseConnectionAsync(Func);

        async Task<bool> Func(DbConnection connection)
        {
            var parameters = new { key, value, expireTime = DateTime.Now.AddMilliseconds(millisecond) };

            return await connection.ExecuteAsync(SqlQueryProvider.GetUpdateExpireQuery(Options.SchemaName), parameters) > 0;
        }
    }

    public bool Delete(string key)
    {
        ClearExpiredValues();
        return UseTransaction((connection, transaction) =>
            connection.Execute(SqlQueryProvider.GetDeleteQuery(Options.SchemaName), new { key }, transaction) > 0);
    }

    #endregion

    #region DbConnection

    protected virtual DbConnection DefaultConnectionFactory()
    {
        var connection = Options.SqlClientFactory.CreateConnection() ??
            throw new InvalidOperationException($"The provider factory ({Options.SqlClientFactory}) returned a null DbConnection.");
        connection.ConnectionString = Options.ConnectionString;
        return connection;
    }

    protected DbConnection CreateAndOpenConnection()
    {
        DbConnection? connection = default;
        try
        {
            connection = ConnectionFactory();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return connection;
        }
        catch
        {
            this.ReleaseConnection(connection);
            throw;
        }
    }


    private void ReleaseConnection(IDbConnection? connection)
    {
        if (connection != null && Options.ConnectionFactory is null)
        {
            connection.Dispose();
        }
    }

    protected void UseConnection(Action<DbConnection> action)
    {
        UseConnection(connection =>
        {
            action(connection);
            return true;
        });
    }


    protected T UseConnection<T>(Func<DbConnection, T> func)
    {
        DbConnection? connection = null;
        try
        {
            connection = CreateAndOpenConnection();
            return func(connection);
        }
        finally
        {
            this.ReleaseConnection(connection);
        }
    }

    protected async Task<T> UseConnectionAsync<T>(Func<DbConnection, Task<T>> func)
    {
        DbConnection? connection = null;
        try
        {
            connection = CreateAndOpenConnection();
            var result = await func(connection);
            return result;
        }
        finally
        {
            this.ReleaseConnection(connection);
        }
    }


    protected T UseTransaction<T>(Func<DbConnection, DbTransaction, T> func, IsolationLevel? isolationLevel = null)
    {
        return UseConnection<T>(connection =>
        {
            using var dbTransaction = connection.BeginTransaction(isolationLevel ?? IsolationLevel.ReadCommitted);
            T obj;
            try
            {
                obj = func(connection, dbTransaction);
                dbTransaction.Commit();
            }
            catch
            {
                if (dbTransaction.Connection != null)
                    dbTransaction.Rollback();
                throw;
            }

            return obj;
        });
    }

    #endregion
}