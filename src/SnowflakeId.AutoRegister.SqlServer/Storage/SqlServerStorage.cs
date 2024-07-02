using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using SnowflakeId.AutoRegister.SqlServer.Configs;
using SnowflakeId.AutoRegister.SqlServer.Core;
using SnowflakeId.AutoRegister.SqlServer.Extensions;

namespace SnowflakeId.AutoRegister.SqlServer.Storage;

/// <summary>
/// Represents a SQL Server storage mechanism for the SnowflakeId AutoRegister system.
/// </summary>
public class SqlServerStorage : IStorage
{
    private readonly SqlServerStorageOptions _options;
    private readonly Func<DbConnection> _connectionFactory;

    internal SqlServerStorage(SqlServerStorageOptions option)
    {
        _options = option ?? throw new ArgumentNullException(nameof(option));
        _options.Validate();

        _connectionFactory = _options.ConnectionFactory ?? DefaultConnectionFactory;
        Initialize();
    }

    #region public Storage

    public bool Exist(string key)
    {
        ClearExpiredValues();
        return UseConnection(connection =>
        {
            var sql = $"SELECT TOP 1 1 FROM [{_options.SchemaName}].[RegisterKeyValues] WHERE [Key] = @key";
            return connection.ExecuteScalar<int>(sql, new { key }) > 0;
        });
    }

    public string? Get(string key)
    {
        ClearExpiredValues();
        return UseConnection(connection =>
        {
            var sql = $"SELECT VALUE FROM [{_options.SchemaName}].[RegisterKeyValues] WHERE [Key] = @key";
            return connection.ExecuteScalar<string?>(sql, new { key });
        });
    }

    public bool Set(string key, string value, int millisecond)
    {
        ClearExpiredValues();
        return UseConnection((connection) =>
        {
            //Insert or Update
            var sql = $"""
                MERGE INTO [{_options.SchemaName}].[RegisterKeyValues] AS Target
                USING (VALUES (@key, @value, @expireTime)) AS Source ([Key], Value, ExpireTime)
                ON Target.[Key] = Source.[Key]
                WHEN MATCHED THEN 
                    UPDATE SET Value = Source.Value, ExpireTime = Source.ExpireTime
                WHEN NOT MATCHED THEN 
                    INSERT ([Key], Value, ExpireTime) VALUES (Source.[Key], Source.Value, Source.ExpireTime);
                """;

            var parameters = new
            {
                key,
                value,
                expireTime = DateTime.Now.AddMilliseconds(millisecond)
            };

            return connection.Execute(sql, parameters) > 0;
        });
    }

    public bool SetNotExists(string key, string value, int millisecond)
    {
        ClearExpiredValues();

        return UseConnection((connection) =>
        {
            var sql = $"""
                INSERT INTO [{_options.SchemaName}].[RegisterKeyValues] ([Key], Value, ExpireTime)
                SELECT @key, @value, @expireTime
                WHERE NOT EXISTS (SELECT 1 FROM [{_options.SchemaName}].[RegisterKeyValues] WHERE [Key] = @key);
                """;

            var parameters = new
            {
                key,
                value,
                expireTime = DateTime.Now.AddMilliseconds(millisecond)
            };

            return connection.Execute(sql, parameters) > 0;
        });
    }

    public bool Expire(string key, int millisecond)
    {
        ClearExpiredValues();
        return UseConnection((connection) =>
        {
            var sql = $"UPDATE [{_options.SchemaName}].[RegisterKeyValues] SET ExpireTime = @expireTime WHERE [Key] = @key";
            var parameters = new
            {
                key,
                expireTime = DateTime.Now.AddMilliseconds(millisecond)
            };

            return connection.Execute(sql, parameters) > 0;
        });
    }

    public Task<bool> ExpireAsync(string key, int millisecond)
    {
        ClearExpiredValues();

        return UseConnectionAsync(Func);

        async Task<bool> Func(DbConnection connection)
        {
            var sql = $"UPDATE [{_options.SchemaName}].[RegisterKeyValues] SET ExpireTime = @expireTime WHERE [Key] = @key";
            var parameters = new { key, expireTime = DateTime.Now.AddMilliseconds(millisecond) };

            return await connection.ExecuteAsync(sql, parameters) > 0;
        }
    }

    public bool Delete(string key)
    {
        ClearExpiredValues();
        return UseTransaction((connection, transaction) =>
        {
            var sql = $"DELETE FROM [{_options.SchemaName}].[RegisterKeyValues] WHERE [Key] = @key";
            return connection.Execute(sql, new { key }, transaction) > 0;
        });
    }

    #endregion

    #region private internal methods

    private void Initialize()
    {
        for (var index = 0; index < 3; ++index)
        {
            try
            {
                this.UseConnection(connection => SqlServerMigrate.Migrate(connection, this._options.SchemaName));
            }
            catch (Exception)
            {
                if (index == 2)
                {
                    throw;
                }
            }
        }
    }

    internal DbConnection DefaultConnectionFactory()
    {
        return new SqlConnection(_options.ConnectionString);
    }


    internal void ClearExpiredValues()
    {
        UseConnection(connection =>
        {
            var sql = $"DELETE FROM [{_options.SchemaName}].[RegisterKeyValues] WHERE ExpireTime < @now";
            return connection.Execute(sql, new { now = DateTime.Now });
        });
    }

    #endregion

    #region SqlConnection

    internal DbConnection CreateAndOpenConnection()
    {
        DbConnection? connection = default;
        try
        {
            connection = _connectionFactory();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return connection;
        }
        catch (Exception ex) when (ex.IsCatchableExceptionType())
        {
            this.ReleaseConnection(connection);
            throw;
        }
    }

    internal void ReleaseConnection(IDbConnection? connection)
    {
        connection?.Dispose();
    }

    internal void UseConnection(Action<DbConnection> action)
    {
        UseConnection(connection =>
        {
            action(connection);
            return true;
        });
    }


    internal T UseConnection<T>(Func<DbConnection, T> func)
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

    internal async Task<T> UseConnectionAsync<T>(Func<DbConnection, Task<T>> func)
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


    internal T UseTransaction<T>(Func<DbConnection, DbTransaction, T> func, IsolationLevel? isolationLevel = null)
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
            catch (Exception ex) when (ex.IsCatchableExceptionType())
            {
                if (dbTransaction.Connection != null)
                    dbTransaction.Rollback();
                throw;
            }

            return obj;
        });
    }

    #endregion

    #region disponse

    public void Dispose()
    {
        // don't have to dispose the connection
    }

    public ValueTask DisposeAsync()
    {
        // don't have to dispose the connection
        return default;
    }

    #endregion
}