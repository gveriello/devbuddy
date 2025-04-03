using System.Text;
using System.Text.Json;
using devbuddy.business.Models;
using devbuddy.common.Models;

namespace devbuddy.business
{
    public class AuthenticationService
    {
        public async Task<string> LoginAsync(string email, string password)
        {
            ArgumentNullException.ThrowIfNull("L'email non può essere vuota.", email);
            ArgumentNullException.ThrowIfNull("La password non può essere vuota.", password);

            var body = new
            {
                Email = email,
                Password = password,
                AppId = Endpoints.APP_ID
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var httpClient = new HttpClient();
            // Invia la richiesta POST
            var response = await httpClient.PostAsync(AuthEndpoints.LOGIN, content);

            // Leggi la risposta
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var httpResponse = JsonSerializer.Deserialize<HttpResponse<string>>(jsonResponse);

            if (response.IsSuccessStatusCode && (httpResponse?.Success ?? false))
                if (!string.IsNullOrEmpty(httpResponse.Data))
                    return httpResponse.Data;

            throw new UnauthorizedAccessException(httpResponse.Message);
        }

    }
}
