using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.cronjob
{
    public static class DependencyInjection
    {
        public static void StartCronJob(this IServiceCollection services)
        {
            BackgroundService.StartAsync(default);
        }
    }
}
