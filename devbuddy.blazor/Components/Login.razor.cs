using Blazored.LocalStorage;
using devbuddy.business;
using devbuddy.business.Models;
using devbuddy.common.Applications;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Components
{
    public sealed partial class Login : PageComponentsBase
    {
        [Inject] public ILocalStorageService LocalStorage { get; set; }
        [Inject] public AuthenticationService AuthenticationService { get; set; }
        [Parameter] public EventCallback OnLogin { get; set; }
        private string? Token { get; set; }
        private readonly LoginRequest Model = new();
        private string Errors { get; set; }
        private bool HasErrors => !string.IsNullOrEmpty(Errors);
        private bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        protected override async Task OnInitializedAsync()
        {
            Token = await LocalStorage.GetItemAsync<string>(nameof(Token)) ?? null;
            LoadingInProgress = false;
            StateHasChanged();
        }

        private async Task SignIn()
        {
            try
            {
                LoadingInProgress = true;
                ShowError();

                this.Token = await AuthenticationService.LoginAsync(Model.Email, Model.Password);
                await LocalStorage.SetItemAsync(nameof(Token), Token);
                await OnLogin.InvokeAsync();
                return;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                LoadingInProgress = false;
            }

            void ShowError(string message = "")
            {
                Errors = message;
                StateHasChanged();
            }
        }
    }
}
