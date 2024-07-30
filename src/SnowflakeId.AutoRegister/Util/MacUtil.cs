using System.Net.NetworkInformation;

namespace SnowflakeId.AutoRegister.Util;

/// <summary>
/// Mac address utility class.
/// </summary>
public class MacUtil
{
    /// <summary>
    /// Gets the MAC address of the current machine.
    /// </summary>
    /// <returns>The MAC address of the current machine.</returns>
    public static string GetMacAddress()
    {
        var mac = string.Empty;
        var nics = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var adapter in nics)
        {
            if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
            mac = adapter.GetPhysicalAddress().ToString();
            break;
        }

        return mac;
    }
}