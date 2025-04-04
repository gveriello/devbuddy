using System.Windows;
using devbuddy.common;
using devbuddy.Desktop.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureDesktopServices();
            serviceCollection.ConfigureComponentsServices();
            serviceCollection.ConfigureCommonServices(false);
            var services = serviceCollection.BuildServiceProvider();
            SplashScreen.ServiceProvider = services;
            Dashboard.ServiceProvider = services;
            base.OnStartup(e);
        }
    }

}
