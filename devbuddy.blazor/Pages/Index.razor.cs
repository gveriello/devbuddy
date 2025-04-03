using System.Reflection;
using Blazored.LocalStorage;
using devbuddy.blazor.Components.Pages;
using devbuddy.business;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.Models;
using devbuddy.common.Services;
using devbuddy.common.Services.Base;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Pages
{
    [Route("/")]
    public partial class Index
    {
        [Inject] private DataModelServiceBase DataModelService { get; set; }
        [Inject] private MemoryCacheService MemoryCacheService { get; set; }
        [Inject] private SidebarNavigationService DashboardState { get; set; }
        [Inject] public ILocalStorageService LocalStorage { get; set; }
        [Inject] protected UserService UserService { get; set; }
        [Inject] protected ToastService ToastService { get; set; }

        private AuthenticatedUser? User;
        private bool IsLogged => this.User is not null;
        private bool Loading { get; set; } = true;
        private Type ComponentToRender = typeof(Home);
        private readonly Dictionary<string, object> ComponentParameters = [];

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                this.User = await UserService.GetUserDataAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                await OnLogout();
            }
            finally
            {
                if (IsLogged)
                {
                    await DataModelService.InitializeAsync();

                    if (!MemoryCacheService.TryGetValueIfIsNotExpired(nameof(Assembly.GetExecutingAssembly), out List<Type> currentAssemblies))
                    {
                        currentAssemblies = Assembly.GetExecutingAssembly().GetTypes()!
                                            .Where(static type => type.GetCustomAttribute<DynamicallyLoadableAttribute>() is not null)?
                                            .ToList();

                        MemoryCacheService.AddOrUpdate(nameof(Assembly.GetExecutingAssembly), currentAssemblies);
                    }

                    DashboardState.SetMenuItem(ModulesItems.Home, ModulesItems.Home.ToString());
                    ComponentToRender = DashboardState.FunctionalityType;
                    ComponentParameters.Clear();
                    ComponentParameters.Add("User", User);
                    Loading = false;
                }
                StateHasChanged();
                await base.OnInitializedAsync();
            }
        }

        private async Task OnLogin()
        {
            await LoadAsync();
        }

        private async Task OnLogout()
        {
            //Logout
            ToastService.Show("La tua sessione è terminata; ti preghiamo di rieffettuare l'accesso.");
            await LocalStorage.ClearAsync();
            this.User = null;
            StateHasChanged();
        }

        private void OnPageChanged()
        {
            Loading = true;
            ComponentToRender = DashboardState.FunctionalityType;
            Loading = false;
            StateHasChanged();
        }
    }
}
