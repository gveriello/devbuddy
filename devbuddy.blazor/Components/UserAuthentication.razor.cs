using Blazored.LocalStorage;
using devbuddy.business;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Components
{
    public sealed partial class UserAuthentication
    {
        [Parameter] public EventCallback ForceLogout { get; set; }
        [Inject] public ILocalStorageService LocalStorage { get; set; }
        [Inject] public TokenService TokenService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CheckTokenAsync();
        }

        private async Task CheckTokenAsync()
        {
            var tokenFromLocalStorage = await LocalStorage.GetItemAsync<string>("Token") ?? null;

            if (string.IsNullOrEmpty(tokenFromLocalStorage))
            {
                await ForceLogoutAsync();
                return;
            }

            var responseFromVerify = await TokenService.VerifyTokenAsync(tokenFromLocalStorage);
            if (!responseFromVerify.isValid)
            {
                await ForceLogoutAsync();
                return;
            }

            await LocalStorage.SetItemAsync("Token", responseFromVerify.newToken);

            async Task ForceLogoutAsync()
            {
                await ForceLogout.InvokeAsync();
                StateHasChanged();
            }
        }
    }
}
