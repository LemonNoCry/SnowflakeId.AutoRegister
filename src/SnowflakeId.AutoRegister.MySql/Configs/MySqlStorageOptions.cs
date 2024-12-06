namespace SnowflakeId.AutoRegister.MySql.Configs;

/// <summary>
/// Represents the options for configuring MySQL in the SnowflakeId AutoRegister system.
/// </summary>
public class MySqlStorageOptions : BaseDbStorageOptions
{
    protected override DbProviderFactory GetDefaultSqlClientFactory()
    {
        var dbProviderFactoryTypes = new[]
        {
            "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data",
            "MySqlConnector.MySqlConnectorFactory, MySqlConnector"
        };

        foreach (var dbProviderFactoryType in dbProviderFactoryTypes)
        {
            var providerFactoryType = Type.GetType(dbProviderFactoryType, throwOnError: false);
            if (providerFactoryType == null) continue;
            var instanceField = providerFactoryType.GetField("Instance");
            if (instanceField == null)
            {
                continue;
            }

            var instance = (DbProviderFactory)instanceField.GetValue(null);
            if (instance != null)
            {
                return instance;
            }
        }

        throw new InvalidOperationException(
            "Please add a NuGet package reference to either 'MySql.Data' or 'MySqlConnector' in your application project. Supports both providers but let the consumer decide which one should be used.");
    }
}