using devbuddy.common.Models;
using Microsoft.AspNetCore.Components;

namespace devbuddy.common.Applications
{
    public abstract class ComponentsBase : ComponentBase
    {
        protected bool LoadingInProgress { get; set; } = false;
        [Parameter] public AuthenticatedUser? User { get; set; }
    }
}
