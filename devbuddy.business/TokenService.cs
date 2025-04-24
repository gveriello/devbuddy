using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using devbuddy.common;

namespace devbuddy.business
{
    public class TokenService
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
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Invia la richiesta POST
            var response = await httpClient.PostAsync(TokenEndpoints.VERIFY_TOKEN, content);

            if (response.IsSuccessStatusCode)
                return (true, await response.Content.ReadAsStringAsync());

            return (false, null);
        }
    }
}
