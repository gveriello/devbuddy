using devbuddy.common.Attributes;
using devbuddy.common.Enums;

namespace devbuddy.blazor.Components.Pages.Plugins
{
    [DynamicallyLoadable(ModulesItems.LoremIpsum)]
    public sealed partial class LoremIpsum
    {
    }
}
