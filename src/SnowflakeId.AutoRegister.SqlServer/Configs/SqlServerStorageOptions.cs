using System.Data.Common;
using SnowflakeId.AutoRegister.Db.Configs;

namespace SnowflakeId.AutoRegister.SqlServer.Configs;

/// <summary>
/// Represents the options for configuring SQL Server in the SnowflakeId AutoRegister system.
/// </summary>
public class SqlServerStorageOptions : BaseDbStorageOptions
{
    protected override DbProviderFactory GetDefaultSqlClientFactory()
    {
        var dbProviderFactoryTypes = new[]
        {
            "Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient",
            // Available in the .NET Framework GAC, requires Version + Culture + PublicKeyToken to be explicitly specified
            "System.Data.SqlClient.SqlClientFactory, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            "System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient",
        };

        foreach (var dbProviderFactoryType in dbProviderFactoryTypes)
        {
            var providerFactoryType = Type.GetType(dbProviderFactoryType, throwOnError: false);
            if (providerFactoryType == null) continue;
            var instanceField = providerFactoryType.GetField("Instance");
            if (instanceField != null)
            {
                var instance = (DbProviderFactory)instanceField.GetValue(null);
                if (instance != null)
                {
                    return instance;
                }
            }

            // If "Instance" field does not exist, try to create an instance directly
            if (typeof(DbProviderFactory).IsAssignableFrom(providerFactoryType))
            {
                return (DbProviderFactory)Activator.CreateInstance(providerFactoryType);
            }
        }

        throw new InvalidOperationException(
            "Please add a NuGet package reference to either 'Microsoft.Data.SqlClient' or 'System.Data.SqlClient' in your application project. Supports both providers but let the consumer decide which one should be used.");
    }
}