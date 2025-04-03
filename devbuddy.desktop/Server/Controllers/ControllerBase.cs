using Microsoft.AspNetCore.Routing;

namespace devbuddy.Desktop.Server.Controllers
{
    internal abstract class ControllerBase
    {
        public abstract void Configure(RouteGroupBuilder apiGroup);
    }
}
