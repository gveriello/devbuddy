using devbuddy.common.Enums;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Components
{
    public sealed partial class MenuNavItem
    {
        [Parameter, EditorRequired] public ModulesItems NavNode { get; set; }
        [Parameter, EditorRequired] public string Icon { get; set; }
        [Parameter, EditorRequired] public string Description { get; set; }
        [Parameter, EditorRequired] public EventCallback<(ModulesItems, string)> OnNavItemClick { get; set; }

        private async Task OnMenuItemClick(ModulesItems node, string description)
            => await OnNavItemClick.InvokeAsync((node, description));

    }
}
