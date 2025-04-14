using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using devbuddy.common;
using devbuddy.common.Models;

namespace devbuddy.business
{
    public class UserService
    {
        private readonly ILocalStorageService _localStorageService;
        private string _token;

        public UserService(ILocalStorageService localStorageService)
        {
            this._localStorageService = localStorageService;
        }

        private async Task LoadTokenIfExists()
            => this._token ??= await _localStorageService.GetItemAsync<string>("Token") ?? throw new UnauthorizedAccessException("Utente non loggato");

        public async Task<AuthenticatedUser> GetUserDataAsync()
        {
            AuthenticatedUser toReturn = null;
            await LoadTokenIfExists();

            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this._token);

            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    AppId = Endpoints.APP_ID
                }),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await client.PostAsync(UserEndpoints.GET_USER_DATA, content);
            HttpResponse<AuthenticatedUser> user = null;

            try
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                user = JsonSerializer.Deserialize<HttpResponse<AuthenticatedUser>>(responseBody);
            }
            finally
            {
                if (response.IsSuccessStatusCode && (user?.Success ?? false))
                {
                    toReturn = user.Data;
                }
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("Non sei autorizzato a poter usare questa funzionalità.");

            return toReturn;
        }



    }
}
