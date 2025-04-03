using System.Text;
using devbuddy.common.Models;
using devbuddy.common.Services.Base;
using Microsoft.JSInterop;

namespace devbuddy.common.Services.Browser
{
    public class BrowserDeviceService : DeviceServiceBase, IAsyncDisposable
    {
        private readonly IJSRuntime jsRuntime;
        private IJSObjectReference? module;
        //, ILocalStorageService LocalStorage
        public BrowserDeviceService(MemoryCacheService memoryCacheService, IJSRuntime jsRuntime) : base(memoryCacheService)
        {
            this.jsRuntime = jsRuntime;
        }

        private async Task<IJSObjectReference> ImportBrowserInfoModule()
        {
            module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/browserInfo.js");
            return module;
        }

        public override async Task<DeviceInfo> HardwareInformationAsync()
        {
            if (!memoryCacheService.TryGetValueIfIsNotExpired<DeviceInfo>(nameof(DeviceInfo), out var toReturn))
            {
                toReturn = new DeviceInfo
                {
                    BrowserInfo = new BrowserInfo()
                };
                try
                {
                    var module = await ImportBrowserInfoModule();
                    toReturn.BrowserInfo = await module.InvokeAsync<BrowserInfo>("getBrowserFingerprint");
                }
                catch (Exception ex)
                {
                    // Gestione degli errori
                    Console.WriteLine($"Error getting browser info: {ex.Message}");
                }

                memoryCacheService.AddOrUpdate(nameof(DeviceInfo), toReturn);
            }
            return toReturn;
        }

        public override byte[] GetDevicePassword()
        {
            return Encoding.UTF8.GetBytes("D3vBu441.B60ws3r");
        }

        public override byte[]? GetSalt()
        {
            return Encoding.UTF8.GetBytes("D3vBu441.B60ws3r");
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }
    }
}
