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
        var adapter = NetworkInterface
           .GetAllNetworkInterfaces()
           .FirstOrDefault(nic =>
                nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                nic.OperationalStatus == OperationalStatus.Up &&
                nic.GetPhysicalAddress().GetAddressBytes().Length > 0);

        return adapter?.GetPhysicalAddress().ToString() ?? string.Empty;
    }
}