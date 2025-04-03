using System.Windows;

namespace devbuddy.NotificationHub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow(string text)
        {
            InitializeComponent();
            this.Hide();
            WindowsNotificationService.Notify(text);
            this.Close();
        }
    }
}