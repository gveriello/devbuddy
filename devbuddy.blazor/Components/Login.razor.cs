using Blazored.LocalStorage;
using devbuddy.business;
using devbuddy.business.Models;
using devbuddy.common.Applications;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.blazor.Components
{
    public sealed partial class Login : PageComponentsBase
    {
        [Inject] public ILocalStorageService LocalStorage { get; set; }
        [Inject] public AuthenticationService AuthenticationService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Parameter] public EventCallback OnLogin { get; set; }
        private string? Token { get; set; }
        private readonly LoginRequest LoginModel = new();
        private readonly RegisterRequest RegisterModel = new();
        private string Errors { get; set; }
        private bool HasErrors => !string.IsNullOrEmpty(Errors);
        private bool IsLoggedIn => !string.IsNullOrEmpty(Token);
        private bool IsRegistering { get; set; } = false;

        private async Task ToggleAuthForm(bool isRegistering)
        {
            // Prima avvia l'animazione di dissolvenza
            await JSRuntime.InvokeVoidAsync("authAnimation.fadeToggle", isRegistering);

            // Cambia lo stato dopo un breve ritardo per consentire all'animazione di iniziare
            await Task.Delay(150);

            IsRegistering = isRegistering;
            ShowError(); // Pulisce eventuali messaggi di errore quando si passa da un form all'altro
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await CheckTokenAsync();
        }

        private async Task CheckTokenAsync()
        {
            LoadingInProgress = true;
            var tokenFromLocalStorage = await LocalStorage.GetItemAsync<string>(nameof(Token)) ?? null;
            if (!await AuthenticationService.VerifyTokenAsync(tokenFromLocalStorage))
            {
                await LocalStorage.ClearAsync();
                Token = null;
            }
            else
            {
                Token = tokenFromLocalStorage;
            }
            StateHasChanged();
            LoadingInProgress = false;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Carica lo script di animazione
                await JSRuntime.InvokeVoidAsync("eval", await GetAuthAnimationScript());
            }

            async Task<string> GetAuthAnimationScript()
            {
                return @"
                window.authAnimation = {
                    fadeToggle: function (isRegistering) {
                        const card = document.querySelector('.form-signin .card');
                        
                        if (card) {
                            // Applica l'effetto di dissolvenza
                            card.style.opacity = '0';
                            card.style.transform = 'translateY(-10px)';
                            
                            // Dopo la dissolvenza, mostra il nuovo contenuto
                            setTimeout(() => {
                                card.style.opacity = '1';
                                card.style.transform = 'translateY(0)';
                            }, 300);
                        }
                    }
                };
            ";
            }
        }


        private async Task SignIn()
        {
            try
            {
                LoadingInProgress = true;
                ShowError();

                this.Token = await AuthenticationService.LoginAsync(LoginModel.Email, LoginModel.Password);
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
        }

        private async Task Register()
        {
            try
            {
                LoadingInProgress = true;
                ShowError();

                // Registrazione utente
                this.Token = await AuthenticationService.RegisterAsync(RegisterModel);
                await LocalStorage.SetItemAsync(nameof(Token), Token);
                await OnLogin.InvokeAsync();
                return;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return;
            }
            finally
            {
                LoadingInProgress = false;
            }
        }

        private void ShowError(string message = "")
        {
            Errors = message;
            StateHasChanged();
        }
    }
}