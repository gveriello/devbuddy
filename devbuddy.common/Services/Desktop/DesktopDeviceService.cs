using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using devbuddy.common.Models;
using devbuddy.common.Services.Base;

namespace devbuddy.common.Services.Desktop
{
    public class DesktopDeviceService(MemoryCacheService memoryCacheService) : DeviceServiceBase(memoryCacheService)
    {
        public override byte[]? GetSalt()
        {
            var deviceInformation = HardwareInformationAsync().Result;

            return Encoding.UTF8.GetBytes(deviceInformation.DesktopInfo.MotherBoardSerialNumber);
        }

        public override byte[] GetDevicePassword()
        {
            var deviceInformation = HardwareInformationAsync().Result;

            var password = new StringBuilder();
            password.Append(deviceInformation.DesktopInfo.BiosSerialNumber);
            password.Append(deviceInformation.DesktopInfo.ProcessorId);

            return Encoding.UTF8.GetBytes(password.ToString());
        }

        public override Task<DeviceInfo> HardwareInformationAsync()
        {
            if (!memoryCacheService.TryGetValueIfIsNotExpired<DeviceInfo>(nameof(DeviceInfo), out var toReturn))
            {
                toReturn = new DeviceInfo
                {
                    DesktopInfo = new DesktopInfo()
                };
                try
                {
                    // Ottieni informazioni sul processore
                    using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                    foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                    {
                        if (obj["ProcessorId"] != null)
                        {
                            toReturn.DesktopInfo.ProcessorId = obj["ProcessorId"]?.ToString();
                        }
                    }
                }
                catch
                {
                }

                try
                {
                    // Ottieni informazioni sulla scheda madre
                    using var searcher = new ManagementObjectSearcher("SELECT SerialNumber,Product FROM Win32_BaseBoard");
                    foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                    {
                        if (obj["SerialNumber"] != null)
                            toReturn.DesktopInfo.MotherBoardSerialNumber = obj["SerialNumber"]?.ToString();
                        if (obj["Product"] != null)
                            toReturn.DesktopInfo.Product = obj["Product"]?.ToString();
                    }
                }
                catch
                {
                }

                try
                {
                    // Ottieni il serial number del BIOS
                    using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
                    foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                    {
                        if (obj["SerialNumber"] != null)
                            toReturn.DesktopInfo.BiosSerialNumber = obj["SerialNumber"]?.ToString();
                    }
                }
                catch
                {
                }

                try
                {
                    // Ottieni informazioni sui dischi fissi
                    using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
                    foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                    {
                        if (obj["SerialNumber"] != null)
                            toReturn.DesktopInfo.DiskSerialNumber = obj["SerialNumber"]?.ToString();
                    }
                }
                catch
                {
                }

                toReturn.DesktopInfo.MachineName = Environment.MachineName;
                toReturn.DesktopInfo.ProcessCount = Environment.ProcessorCount;
                toReturn.DesktopInfo.OSDescription = RuntimeInformation.OSDescription;
                toReturn.DesktopInfo.ProcessArchitecture = RuntimeInformation.ProcessArchitecture;

                memoryCacheService.AddOrUpdate(nameof(DeviceInfo), toReturn);
            }

            return Task.FromResult(toReturn);
        }
    }
}
