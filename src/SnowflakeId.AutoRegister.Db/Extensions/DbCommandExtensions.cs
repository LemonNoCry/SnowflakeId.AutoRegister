namespace SnowflakeId.AutoRegister.Db.Extensions;

/// <summary>
/// Provides extension methods for executing commands on DbConnection.
/// </summary>
public static class DbCommandExtensions
{
    public static int ExecuteNonQuery(this DbConnection connection, string query, object? parameters, DbTransaction? transaction = null)
    {
        using var command = connection.CreateCommand(query, parameters, transaction);
        return command.ExecuteNonQuery();
    }

    public static async Task<int> ExecuteNonQueryAsync(this DbConnection connection, string query, object? parameters,
        DbTransaction? transaction = null)
    {
        using var command = connection.CreateCommand(query, parameters, transaction);
        return await command.ExecuteNonQueryAsync();
    }

    public static int Execute(this DbConnection connection, string query, object? parameters = default, DbTransaction? transaction = null)
    {
        return connection.ExecuteNonQuery(query, parameters, transaction);
    }

    public static async Task<int> ExecuteAsync(this DbConnection connection, string query, object? parameters, DbTransaction? transaction = null)
    {
        return await connection.ExecuteNonQueryAsync(query, parameters, transaction);
    }

    public static T? ExecuteScalar<T>(this DbConnection connection, string query, object? parameters, DbTransaction? transaction = null)
    {
        using var command = connection.CreateCommand(query, parameters, transaction);
        var result = command.ExecuteScalar();

        if (result == null || result == DBNull.Value)
        {
            return default;
        }

        return (T)Convert.ChangeType(result, typeof(T));
    }

    private static DbCommand CreateCommand(this DbConnection connection, string query, object? parameters, DbTransaction? transaction = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = query;
        command.Transaction = transaction;
        if (parameters != null)
            command.AddParameters(parameters);
        return command;
    }

    private static void AddParameters(this DbCommand command, object parameters)
    {
        foreach (var property in parameters.GetType().GetProperties())
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = property.Name;
            parameter.Value = property.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}