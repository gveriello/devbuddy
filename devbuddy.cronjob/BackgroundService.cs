using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Hosting;
using devbuddy.common;
using devbuddy.common.Services;

namespace devbuddy.cronjob
{
    public class BackgroundService
    {
        public static async Task StartAsync(CancellationToken cancellationToken)
        {
            var featureFlagsService = new FeatureFlagsService();
            var canStartCronJobFF = await featureFlagsService.GetAsync("CanStartCronJob", "false");
            _ = bool.TryParse(canStartCronJobFF, out bool canStartCronJob);
            if (!canStartCronJob)
                return;

            var tickCronJobSecondsFF = await featureFlagsService.GetAsync("TickCronJobSeconds", "60");
            _ = int.TryParse(tickCronJobSecondsFF, out int seconds);

            while (true)
            {

                await Task.Delay(seconds * 1000);
            }
        }

        public static Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
