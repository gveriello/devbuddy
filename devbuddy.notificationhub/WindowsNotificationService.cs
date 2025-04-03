using Microsoft.Toolkit.Uwp.Notifications;

namespace devbuddy.NotificationHub
{
    public class WindowsNotificationService
    {
        public static void Notify(string text)
        {
            if (string.IsNullOrEmpty(text)) { return; }

            new ToastContentBuilder()
                .SetToastDuration(ToastDuration.Short)
                .AddText("DevBuddy")
                .AddText(text)
                .SetToastScenario(ToastScenario.Reminder)
                .Show();
        }
    }
}
