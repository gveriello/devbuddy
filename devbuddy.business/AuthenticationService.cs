using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using devbuddy.business.Models;
using devbuddy.common;
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

        public async Task<bool> VerifyTokenAsync(string token)
        {
            ArgumentNullException.ThrowIfNull("Token non valido.", token);

            var content = new StringContent(
                JsonSerializer.Serialize(new {}),
                Encoding.UTF8,
                "application/json"
            );

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Invia la richiesta POST
            var response = await httpClient.PostAsync(TokenEndpoints.VERIFY_TOKEN, content);

            return response.IsSuccessStatusCode;
        }

        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            ArgumentNullException.ThrowIfNull("Il nome non può essere vuoto.", request.Name);
            ArgumentNullException.ThrowIfNull("Il cognome non può essere vuoto.", request.Surname);
            ArgumentNullException.ThrowIfNull("L'email non può essere vuota.", request.Email);
            ArgumentNullException.ThrowIfNull("La password non può essere vuota.", request.Password);

            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("Le password non coincidono.");

            var body = new
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                Password = request.Password,
                AppId = Endpoints.APP_ID
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var httpClient = new HttpClient();
            // Invia la richiesta POST
            var response = await httpClient.PostAsync(AuthEndpoints.REGISTER, content);

            // Leggi la risposta
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var httpResponse = JsonSerializer.Deserialize<HttpResponse<string>>(jsonResponse);

            if (response.IsSuccessStatusCode && (httpResponse?.Success ?? false))
                if (!string.IsNullOrEmpty(httpResponse.Data))
                    return httpResponse.Data;

            throw new Exception(httpResponse?.Message ?? "Errore durante la registrazione.");
        }
    }
}
