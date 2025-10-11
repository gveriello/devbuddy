using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using devbuddy.common;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.ExtensionMethods;
using devbuddy.common.Models;
using devbuddy.common.Services;

namespace devbuddy.business
{
    public class SidebarService(DevUtilityService devUtilityService, ILocalStorageService _localStorageService)
    {
        public async Task<List<NavItem>> GetAllAsync()
        {
            string _token = await _localStorageService.GetItemAsync<string>("Token") ?? throw new UnauthorizedAccessException("Utente non loggato");

            HttpClient httpClient = new();
            devUtilityService.InjectDevEnvironment(httpClient);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    AppId = Endpoints.APP_ID
                }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync(ModulesEndpoints.GET_APP_MODULES, content);

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
                        Id = isHome ? (int)@enum : module!.Id,
                        Node = @enum,
                        Name = isHome ? @enum.ToString() : module!.Name,
                        Description = isHome ? ModulesItems.Home.ToString() : module!.Description,
                        Icon = isHome ? @enum.AttributeValueOrDefault<IconAttribute, string>(attr => attr.Icon) : module!.Icon,
                        ApiKey = isHome ? null : module!.ApiKey,
                        Category = isHome ? null : module!.Category
                    };
                    toReturn.Add(toAdd);
                }
            }


            return toReturn;
        }
    }
}
