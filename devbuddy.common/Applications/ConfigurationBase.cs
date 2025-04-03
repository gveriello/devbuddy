using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.common.Applications
{
    public abstract class ConfigurationBase : NavItemBase
    {
        protected void RegisterService<TService, TImplementation>(IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TService : class
            where TImplementation : class, TService
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<TService, TImplementation>();
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped<TService, TImplementation>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<TService, TImplementation>();
                    break;
            }
        }

        protected void RegisterService<TService>(IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TService : class
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<TService>();
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped<TService>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<TService>();
                    break;
            }
        }
    }

}
