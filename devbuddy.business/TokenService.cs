using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using devbuddy.common;
using devbuddy.common.Models;
using devbuddy.common.Services;

namespace devbuddy.business
{
    public class TokenService(DevUtilityService devUtilityService)
    {

        public async Task<(bool isValid, string? newToken)> VerifyTokenAsync(string token)
        {
            ArgumentNullException.ThrowIfNull("Token non valido.", token);

            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    AppId = Endpoints.APP_ID
                }),
                Encoding.UTF8,
                "application/json"
            );

            var httpClient = new HttpClient();
            devUtilityService.InjectDevEnvironment(httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Invia la richiesta POST
            var response = await httpClient.PostAsync(TokenEndpoints.VERIFY_TOKEN, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var httpResponse = JsonSerializer.Deserialize<HttpResponse<string>>(jsonResponse);
                return (true, httpResponse!.Data);
            }

            return (false, null);
        }
    }
}
