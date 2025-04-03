using devbuddy.common.Models;

namespace devbuddy.common.Services.Base
{
    public abstract class DeviceServiceBase
    {
        public static bool IsInstanceInWebApp { get; set; } = true;

        protected readonly MemoryCacheService memoryCacheService;
        public DeviceServiceBase(MemoryCacheService memoryCacheService)
        {
            this.memoryCacheService = memoryCacheService;
        }

        public abstract Task<DeviceInfo> HardwareInformationAsync();
        public abstract byte[] GetDevicePassword();
        public abstract byte[]? GetSalt();
    }

}
