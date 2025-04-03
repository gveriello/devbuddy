using System.Windows;

namespace devbuddy.NotificationHub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string[] args = e.Args;
            if (args.Length == 1)
            {
                var mainWindow = new MainWindow(args[0]);
                mainWindow.Show();
            }
            base.OnStartup(e);
        }
    }

}
