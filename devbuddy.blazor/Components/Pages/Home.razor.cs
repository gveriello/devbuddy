using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Components.Pages
{
    [DynamicallyLoadable(ModulesItems.Home)]
    public partial class Home : PageComponentsBase
    {
        [Inject] TasksExecuterService TasksExecuterService { get; set; }

        protected override async Task OnInitializedAsync()
        {
        }
    }
}
