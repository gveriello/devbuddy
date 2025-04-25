using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Hosting;
using devbuddy.common;

namespace devbuddy.cronjob
{
    public class BackgroundService
    {
        public static async Task StartAsync(CancellationToken cancellationToken)
        {
            while(true)
            {

                var content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        AppId = Endpoints.APP_ID
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var httpClient = new HttpClient();
                // Invia la richiesta POST
                var response = await httpClient.PostAsync(KeysEndpoints.TEST, content, cancellationToken);
                await Task.Delay(20000);
            }
        }

        public static Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
