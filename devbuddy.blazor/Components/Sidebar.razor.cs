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
        private List<NavItem> AllModulesFiltered = [];
        string _searchTool;
        private string SearchTool {
            get => _searchTool;
            set => OnSearchToolValueChanged(value);
        }

        private void OnSearchToolValueChanged(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                AllModulesFiltered = AllModules.ToList();
            }
            else
            {
                AllModulesFiltered = [.. AllModules.Where(module => module.Description.ToUpper().Contains(newValue.ToUpper()) || 
                                                                module.Node is ModulesItems.Home)];
            }

            this._searchTool = newValue;
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadItems();
        }

        private async Task LoadItems()
        {
            try
            {
                AllModules = AllModulesFiltered = await SidebarService.GetAllAsync();
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
