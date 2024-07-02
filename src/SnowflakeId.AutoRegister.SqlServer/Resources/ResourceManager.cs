using System.Reflection;

namespace SnowflakeId.AutoRegister.SqlServer.Resources;

public static class ResourceManager
{
    /// <summary>
    /// Gets the migration script for SqlServer.
    /// </summary>
    public static string MigrateScript => GetStringResource("Snowflake.Migrate.SqlServer.sql");


    #region private

    private static string GetStringResource(string resourceName)
    {
        var assembly = typeof(ResourceManager).GetTypeInfo().Assembly;
        var resources = $"{assembly.GetName().Name}.Resources.{resourceName}";
        using var manifestResourceStream = assembly.GetManifestResourceStream(resources);
        if (manifestResourceStream == null)
            throw new InvalidOperationException($"Requested resource `{resources}` was not found in the assembly `{assembly}`.");
        using var streamReader = new StreamReader(manifestResourceStream);
        return streamReader.ReadToEnd();
    }

    #endregion
}