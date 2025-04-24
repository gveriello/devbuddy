using Blazored.LocalStorage;
using devbuddy.business;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Components
{
    public sealed partial class UserAuthentication
    {
        [Parameter] public EventCallback ForceLogout { get; set; }
        [Inject] public ILocalStorageService LocalStorage { get; set; }
        [Inject] public AuthenticationService AuthenticationService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CheckTokenAsync();
        }
        private async Task CheckTokenAsync()
        {
            var tokenFromLocalStorage = await LocalStorage.GetItemAsync<string>("Token") ?? null;
            if (string.IsNullOrEmpty(tokenFromLocalStorage) || !await AuthenticationService.VerifyTokenAsync(tokenFromLocalStorage))
            {
                await LocalStorage.ClearAsync();
                await ForceLogout.InvokeAsync();
            }
            StateHasChanged();
        }
    }
}
