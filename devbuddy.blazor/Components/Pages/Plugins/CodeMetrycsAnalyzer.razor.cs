using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;

namespace devbuddy.blazor.Components.Pages.Plugins
{
    [DynamicallyLoadable(ModulesItems.CodeMetricsAnalyzer)]
    public sealed partial class CodeMetricsAnalyzer : PageComponentsBase
    {
        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }
    }
}