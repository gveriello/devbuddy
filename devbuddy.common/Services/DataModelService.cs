using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using devbuddy.common.Applications;
using devbuddy.common.Exceptions;
using devbuddy.common.Models;
using devbuddy.common.Services.Base;

namespace devbuddy.common.Services
{
    public class DataModelService(MemoryCacheService memoryCacheService, ILocalStorageService localStorage, DevUtilityService devUtilityService)
    {
        protected bool _isDirty, _isInitialized;
        protected readonly object _lock = new();
        protected readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private string BuildDataModelKey(string token, string apiKey)
            => $"{{tok:{token}}}{{api:{apiKey}}}";

        public async Task<TCustomDataModel> GetDataModelByApiKey<TCustomDataModel>(string apiKey)
            where TCustomDataModel : CustomDataModelBase, new()
        {
            try
            {
                string _token = await localStorage.GetItemAsync<string>("Token") ?? throw new UnauthorizedAccessException("Utente non loggato");

                if (memoryCacheService.TryGetValueIfIsNotExpired(BuildDataModelKey(_token, apiKey), out string dataModel))
                {
                    return JsonSerializer.Deserialize<TCustomDataModel>(dataModel, _options);
                }

                HttpClient client = new();
                devUtilityService.InjectDevEnvironment(client);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    AppId = Endpoints.APP_ID,
                    ApiKey = apiKey
                }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(DataModelEndpoints.GET_DATAMODEL, content);

                // Leggi la risposta
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new TCustomDataModel();

                var httpResponse = JsonSerializer.Deserialize<HttpResponse<string>>(jsonResponse);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(httpResponse.Message);

                if (!string.IsNullOrEmpty(httpResponse.Data))
                    return JsonSerializer.Deserialize<TCustomDataModel>(httpResponse.Data, _options);

                return new();
            }
            catch (Exception ex)
            {
                throw new DataModelServiceException("Errore durante il salvataggio dei dati.", ex);
            }
        }

        public async Task SaveChangesAsync<TCustomDataModel>(string apiKey, TCustomDataModel dataModel)
            where TCustomDataModel : CustomDataModelBase, new()
        {
            try
            {
                string _token = await localStorage.GetItemAsync<string>("Token") ?? throw new UnauthorizedAccessException("Utente non loggato");

                memoryCacheService.AddOrUpdate(BuildDataModelKey(_token, apiKey), JsonSerializer.Serialize(dataModel, _options));

                HttpClient httpClient = new();
                devUtilityService.InjectDevEnvironment(httpClient);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var toSave = JsonSerializer.Serialize(dataModel, _options);

                var requestPayload = new Dictionary<string, string>
                {
                    { "AppId", Endpoints.APP_ID },
                    { "ApiKey", apiKey },
                    { "DataModel", Convert.ToBase64String(Encoding.UTF8.GetBytes(toSave)) }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestPayload, _options),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync(DataModelEndpoints.UPSERT_DATAMODEL, content);

                // Leggi la risposta
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var httpResponse = JsonSerializer.Deserialize<HttpResponse<List<NavItem>>>(jsonResponse);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(httpResponse.Message);

                _isDirty = false;
            }
            catch (Exception ex)
            {
                throw new DataModelServiceException("Errore durante il salvataggio dei dati.", ex);
            }
        }
    }
}
