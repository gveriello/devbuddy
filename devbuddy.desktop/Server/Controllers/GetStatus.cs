using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace devbuddy.Desktop.Server.Controllers
{
    internal class GetStatus : ControllerBase
    {
        public override void Configure(RouteGroupBuilder apiGroup)
        {
            apiGroup.MapGet("/status", () =>
            {
                return TypedResults.Ok(new
                {
                    Status = "Running",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0"
                });
            })
            .WithName("GetStatus")
            .WithDescription("Restituisce lo stato del server");
        }
    }
}
