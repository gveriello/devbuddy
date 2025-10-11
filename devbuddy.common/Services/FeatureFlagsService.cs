using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using devbuddy.common.Models;

namespace devbuddy.common.Services
{
    public class FeatureFlagsService(DevUtilityService devUtilityService)
    {
        public async Task<string?> GetAsync(string key, object? defaultValue = null)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        AppId = Endpoints.APP_ID,
                        Key = key
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var httpClient = new HttpClient();
                devUtilityService.InjectDevEnvironment(httpClient);
                var response = await httpClient.PostAsync(FeatureFlagEndpoints.GET, content);
                // Leggi la risposta
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return defaultValue?.ToString();

                var httpResponse = JsonSerializer.Deserialize<HttpResponse<string>>(jsonResponse);
                return httpResponse!.Data;
            }
            catch
            {
                return default;
            }
        }
    }
}
