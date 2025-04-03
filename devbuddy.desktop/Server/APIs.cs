using devbuddy.Desktop.Server.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace devbuddy.Desktop.Server
{
    public class APIs
    {
        public static void ConfigureApiEndpoints(WebApplication app)
        {
            var apiGroup = app.MapGroup("/api/v1")
                             .WithOpenApi()
                             .WithTags("DevBuddy API")
                             .RequireRateLimiting("fixed");

            new GetStatus().Configure(apiGroup);
        }
    }
}
