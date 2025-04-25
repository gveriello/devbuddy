using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.cronjob
{
    public static class DependencyInjection
    {
        public static async void StartCronJob(this IServiceCollection services)
        {
            await BackgroundService.StartAsync(default);
        }
    }
}
