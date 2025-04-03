using devbuddy.common.Applications;
using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.Plugins.PomodoroTimer.Configurations
{
    public class Configuration : ConfigurationBase
    {
        public Configuration(IServiceCollection serviceCollection)
        {
            Description = nameof(PomodoroTimer);
            Icon = "fa-clock";
        }
    }
}
