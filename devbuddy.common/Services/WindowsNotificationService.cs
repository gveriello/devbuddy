using System.Diagnostics;

namespace devbuddy.common.Services
{
    public class WindowsNotificationService
    {
        public static async Task NotifyAsync(string body, bool wait = false)
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), SpecialPaths.NOTIFICATION_HUB)))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SpecialPaths.NOTIFICATION_HUB,
                        Arguments = body,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                process.Start();
                if (wait)
                    await process.WaitForExitAsync();
            }
        }
    }
}
