using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.business
{
    public static class DependencyInjection
    {
        public static void ConfigureComponentsServices(this IServiceCollection services)
        {
            services.AddScoped<SidebarService>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<TokenService>();
            services.AddScoped<UserService>();
            services.AddScoped<AnalyticsService>();
        }
    }
}
