using devbuddy.business;
using devbuddy.common.Applications;
using devbuddy.common.Enums;
using devbuddy.common.Models;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Components
{
    public partial class Sidebar : PageComponentsBase
    {
        [Inject] public SidebarService SidebarService { get; set; }
        [Inject] private SidebarNavigationService DashboardState { get; set; }

        [Parameter] public EventCallback OnLogout { get; set; }
        [Parameter] public EventCallback OnPageChanged { get; set; }

        private List<NavItem> AllModules = [];

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadItems();
        }

        private async Task LoadItems()
        {
            try
            {
                AllModules = await SidebarService.GetAllAsync();
                StateHasChanged();
            }
            catch(UnauthorizedAccessException)
            {
                await OnLogout.InvokeAsync();
            }
        }

        private void OnMenuItemClick((ModulesItems navNode, string navDescription) node)
        {
            DashboardState.SetMenuItem(node.navNode, node.navDescription);
            OnPageChanged.InvokeAsync();
        }
    }
}
