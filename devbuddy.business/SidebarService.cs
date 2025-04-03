using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using devbuddy.business.Models;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.ExtensionMethods;
using devbuddy.common.Models;

namespace devbuddy.business
{
    public class SidebarService
    {
        private readonly ILocalStorageService _localStorageService;
        private string _token;

        public SidebarService(ILocalStorageService localStorageService)
        {
            this._localStorageService = localStorageService;
        }

        private async Task LoadTokenIfExists()
            => this._token ??= await _localStorageService.GetItemAsync<string>("Token");

        public async Task<List<NavItem>> GetAllAsync()
        {
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

            var response = await client.PostAsync(ModulesEndpoints.GET_APP_MODULES, content);

            // Leggi la risposta
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var httpResponse = JsonSerializer.Deserialize<HttpResponse<List<NavItem>>>(jsonResponse);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException(httpResponse.Message);

            var toReturn = new List<NavItem>();

            var allItems = Enum.GetValues<ModulesItems>();
            foreach (var @enum in allItems)
            {
                if (@enum is ModulesItems.Unparented) continue;

                var isHome = @enum is ModulesItems.Home;
                var module = httpResponse!.Data?.FirstOrDefault(item => item.ApiKey == @enum.AttributeValueOrDefault<ModuleKeyAttribute, string>(attr => attr.Key));
                if (isHome || module is not null)
                {
                    var toAdd = new NavItem()
                    {
                        Node = @enum,
                        Description = @enum.AttributeValueOrDefault<DescriptionAttribute, string>(attr => attr.Description) ?? @enum.ToString(),
                        Icon = isHome ? @enum.AttributeValueOrDefault<IconAttribute, string>(attr => attr.Icon) : module!.Icon
                    };
                    toReturn.Add(toAdd);
                }
            }


            return toReturn;
        }
    }
}
