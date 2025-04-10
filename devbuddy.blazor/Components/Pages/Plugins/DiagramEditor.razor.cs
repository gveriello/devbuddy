using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;

namespace devbuddy.blazor.Components.Pages.Plugins
{
    [DynamicallyLoadable(ModulesItems.DiagramEditor)]
    public sealed partial class DiagramEditor : PageComponentsBase
    {
    }
}
