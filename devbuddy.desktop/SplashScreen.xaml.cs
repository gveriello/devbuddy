using System.Windows;
using devbuddy.common.Services.Base;
using devbuddy.Desktop.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace devbuddy.Desktop
{
    public partial class SplashScreen : Window
    {
        public static ServiceProvider ServiceProvider { get; internal set; }

        public SplashScreen()
        {
            InitializeComponent();
            PrepareDataModel();
            this.Loaded += SplashScreen_Loaded;
        }

        private static void PrepareDataModel()
        {
            _ = ServiceProvider.GetRequiredService<DataModelServiceBase>();
        }

        private void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var dashboardWindow = new Dashboard();
            dashboardWindow.Show();
            this.Close();
        }
    }
}