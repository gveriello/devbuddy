using System.Timers;
using devbuddy.common.Services.Base;
using Microsoft.Win32;

namespace devbuddy.common.Services
{
    public static class TimerService
    {
        static private System.Timers.Timer timer;
        static private DateTime? startTime, pauseTime;

        public static TimeSpan TotalLockTime;
        public static TimeSpan ElapsedTime;
        public static bool InProgress;

        public static event Action? OnWindowsActivityTick;
        public static event Action? OnWindowsInactivityTick;

        public static void Start()
        {
            startTime = DateTime.Now;
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += TimerElapsed;
            timer.AutoReset = true;
            timer.Start();
            InProgress = true;

            if (!DeviceServiceBase.IsInstanceInWebApp)
            {
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            }
        }

        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    PauseTimer();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    ResumeTimer();
                    break;
            }
        }

        private static void PauseTimer()
        {
            pauseTime = DateTime.Now;
            timer.Stop();
            InProgress = false;
        }

        private static void ResumeTimer()
        {
            if (pauseTime.HasValue)
            {
                var lockDuration = DateTime.Now - pauseTime.Value;
                TotalLockTime += lockDuration;
                OnWindowsInactivityTick?.Invoke();

                startTime = startTime!.Value.Add(lockDuration);
                pauseTime = null;
                timer.Start();
                InProgress = true;
            }
        }

        public static void Stop()
        {
            timer.Stop();
            InProgress = false;
        }

        private static void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            ElapsedTime = DateTime.Now - startTime.GetValueOrDefault();
            OnWindowsActivityTick?.Invoke();
        }
    }
}
