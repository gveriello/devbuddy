using Blazored.LocalStorage;

namespace devbuddy.business
{
    public class AnalyticsService
    {
        private readonly ILocalStorageService _localStorageService;
        public AnalyticsService(ILocalStorageService _localStorageService)
        {
            this._localStorageService = _localStorageService;
        }

        public async Task<bool> CanCollectAnalyticsData()
        {
            var analyticsEnabled = await _localStorageService.GetItemAsync<string>("analyticsEnabled");
            return analyticsEnabled != "false";
        }
    }
}
