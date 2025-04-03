using devbuddy.common.Applications;
using devbuddy.Plugins.MyNotes.Services;
using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.Plugins.MyNotes.Configurations
{
    public class Configuration : ConfigurationBase
    {
        public Configuration(IServiceCollection serviceCollection)
        {
            Description = "MyNotes";
            Icon = "fa-notes";

            RegisterService<MyNotesService>(serviceCollection);
        }
    }
}
