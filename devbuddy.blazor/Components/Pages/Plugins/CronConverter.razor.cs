using devbuddy.common.Attributes;
using devbuddy.common.Enums;

namespace devbuddy.blazor.Components.Pages.Plugins
{
    [DynamicallyLoadable(ModulesItems.CronExpression)]
    public sealed partial class CronConverter
    {
    }
}
