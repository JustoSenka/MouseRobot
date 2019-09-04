using Robot.Analytics.Abstractions;
using RobotRuntime;
using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Unity.Lifetime;

namespace Robot.Analytics
{
    [RegisterTypeToContainer(typeof(IUserIdentity), typeof(ContainerControlledLifetimeManager))]
    public class UserIdentity : IUserIdentity
    {
        readonly private INetwork Network;
        public UserIdentity(INetwork Network)
        {
            this.Network = Network;
        }

        /// <summary>
        /// Returns string representing operating system
        /// </summary>
        public string GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "OSX";

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";

            else
                return "Unknown";
        }

        /// <summary>
        /// Gets virtual screen space resolution.
        /// Multiple monitors are treaded as one big virtual monitor.
        /// </summary>
        public string GetScreenResolution()
        {
            return SystemInformation.VirtualScreen.Width + "x" + SystemInformation.VirtualScreen.Height;
        }

        /// <summary>
        /// Fetches user ID from the licensing system.
        /// NOT YET IMPLEMENTED
        /// </summary>
        public string GetUserID()
        {
            return "0"; // Look if user license is active and is registered. Get his ID from the license info
        }

        /// <summary>
        /// Tries to get CPU ID at first, if fails, returns MAC address, if that fails, returns 0
        /// Hashes the result, and returns 64bit int representation of the hash.
        /// </summary>
        public string GetMachineID()
        {
            var id = GetCpuID();
            if (string.IsNullOrEmpty(id))
                id = GetMacAddress();

            if (string.IsNullOrEmpty(id))
                return 0 + "";

            var idBytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(id));
            var guid = new Guid(idBytes);

            return "00" + guid.ToString().Substring(8, 22);
            // First 4 chars because they are random, since we provided only 16 bytes from hash to guid, which takes 20 bytes
            // Skipping 8 chars which are random, (4 are from hash but we don't care)
            // Adding 2 zeros in front for unregistered clients.
        }

        /// <summary>
        /// Gets cpu information and returns everything in one line as a string. 
        /// If not found, returns emnpty string
        /// </summary>
        public string GetCpuID()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

                foreach (ManagementObject queryObj in searcher.Get())
                    return string.Join(", ", queryObj["Architecture"], queryObj["Caption"], queryObj["Family"], queryObj["ProcessorId"]);

                Logger.Log(LogType.Warning, "No CPU information found to identify user.");
                return "";
            }
            catch (ManagementException e)
            {
                Logger.Log(LogType.Warning, "Cannot get cpu description to identify user: " + e.Message);
                return "";
            }
        }

        /// <summary>
        /// Finds the MAC address of the NIC with maximum speed.
        /// If not found, returns empty string
        /// </summary>
        public string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            var macAddress = string.Empty;
            var maxSpeed = -1L;

            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    string tempMac = nic.GetPhysicalAddress().ToString();
                    if (nic.Speed > maxSpeed && !string.IsNullOrEmpty(tempMac) && tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                    {
                        maxSpeed = nic.Speed;
                        macAddress = tempMac;
                    }
                }

                return macAddress;
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Warning, "Cannot get mac address to identify user: " + e.Message);
                return "";
            }
        }

        /// <summary>
        /// Returns country ID based on the external ip of the machine.
        /// Returns empty string if exception or could not detect ip or id
        /// </summary>
        public string GetCountryID() => Network.GetCountryID();
    }
}
