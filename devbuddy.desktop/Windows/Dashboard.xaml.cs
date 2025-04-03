using System.Windows;
using devbuddy.common.Services.Base;
using devbuddy.Desktop.Server;
using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.Desktop.Windows
{
    public partial class Dashboard : Window
    {
        public static ServiceProvider ServiceProvider { get; internal set; }
        private readonly Host Host = new();

        public Dashboard()
        {
            this.Load();
            this.Initialized += Dashboard_Initialized;
            this.Loaded += Dashboard_Loaded;
            this.Closed += Dashboard_Closed;
            this.Closing += Dashboard_Closing;

            InitializeComponent();
        }


        private async void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = ServiceProvider.GetRequiredService<DataModelServiceBase>()?.GetSettings();

            if (settings?.AuthorizeAnonymousSend ?? false)
                await Host.StartAsync();
        }

        private void Load()
        {
            Resources.Add("services", ServiceProvider);
        }

        private void Dashboard_Initialized(object? sender, System.EventArgs e)
        {
        }

        private void Dashboard_Closed(object? sender, System.EventArgs e) => Application.Current.Shutdown();
        private async void Dashboard_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Temporaneamente previene la chiusura
            await Host.StopAsync();
            e.Cancel = false; // Permette la chiusura dopo lo stop del server
        }
    }
}
