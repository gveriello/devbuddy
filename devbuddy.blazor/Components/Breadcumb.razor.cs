using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;

namespace devbuddy.blazor.Components
{
    public partial class Breadcumb
    {
        [Parameter] public SidebarNavigationService DashboardState { get; set; }
    }
}
