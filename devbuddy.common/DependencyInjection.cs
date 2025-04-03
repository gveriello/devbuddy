using System.Text.Json;
using Blazored.LocalStorage;
using devbuddy.common.Services;
using devbuddy.common.Services.Base;
using devbuddy.common.Services.Browser;
using devbuddy.common.Services.Desktop;
using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.common
{
    public static class DependencyInjection
    {
        public static void ConfigureCommonServices(this IServiceCollection services, bool isInstanceInWebApp = true)
        {
            DeviceServiceBase.IsInstanceInWebApp = isInstanceInWebApp;

            services
                    .AddSingleton<MemoryCacheService>()
                    .AddSingleton<LoggerService>()
                    .AddScoped<TasksExecuterService>()
                    .AddScoped<LoggerService>()
                    .AddScoped<ToastService>()
                    .AddScoped<SidebarNavigationService>()
                    .AddBlazoredLocalStorage();
            ;

            if (DeviceServiceBase.IsInstanceInWebApp)
            {
                services.RegisterBrowserServices();
            }
            else
            {
                services.RegisterDesktopServices();
            }
        }

        private static void RegisterBrowserServices(this IServiceCollection services)
        {
            services.AddScoped<DeviceServiceBase, BrowserDeviceService>();
            services.AddScoped<DataModelServiceBase, BrowserDataModelService>();
            services.AddScoped<EncryptionServiceBase, BrowserEncryptionService>();
            services.AddBlazoredLocalStorage(config =>
            {
                config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                config.JsonSerializerOptions.WriteIndented = false;
            });
        }

        private static void RegisterDesktopServices(this IServiceCollection services)
        {
            services.AddScoped<DeviceServiceBase, DesktopDeviceService>();
            services.AddScoped<DataModelServiceBase, DesktopDataModelService>();
            services.AddScoped<EncryptionServiceBase, DesktopEncryptionService>();
        }
    }
}
