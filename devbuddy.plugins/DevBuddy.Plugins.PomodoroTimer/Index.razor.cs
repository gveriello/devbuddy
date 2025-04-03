using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.Services;
using devbuddy.Plugins.PomodoroTimer.Models;

namespace devbuddy.Plugins.PomodoroTimer
{
    //[DynamicallyLoadable(ModulesItems.PomodoroTimer)]
    public partial class Index : AppComponentBase<PomodoroTimerDataModel>
    {
        private TimeSpan elapsedTime, lockTime;

        private int Hours => (int)elapsedTime.TotalHours;
        private int Minutes => elapsedTime.Minutes;
        private int Seconds => elapsedTime.Seconds;
        private bool HasActivityRecorded => Hours > 0 || Minutes > 0 || Seconds > 0;

        private int LockHours => (int)lockTime.TotalHours;
        private int LockMinutes => lockTime.Minutes;
        private int LockSeconds => lockTime.Seconds;
        private bool HasInactivityRecorded => LockHours > 0 || LockMinutes > 0 || LockSeconds > 0;

        private ModalComponentBase _reportsModal;
        private async Task OnReportsModalOk()
        {

        }

        private async Task OnReportsModalClose()
        {

        }

        protected override void OnInitialized()
        {
            Model = DataModelService.ValueByKey<PomodoroTimerDataModel>(nameof(PomodoroTimer));
            TimerService.OnWindowsActivityTick += OnTickEvent;
            TimerService.OnWindowsInactivityTick += OnLockTimeEvent;
            elapsedTime = TimerService.ElapsedTime;
            ToastService.Position = ToastPosition.BottomRight;
        }

        private void StartMonitoring()
        {
            if (!TimerService.InProgress) TimerService.Start();
        }

        private void StopMonitoring()
        {
            if (TimerService.InProgress) TimerService.Stop();
        }

        private async Task SaveMonitoringAsync()
        {
            if (!TimerService.InProgress && !HasActivityRecorded && !HasInactivityRecorded)
            {
                ToastService.Show("Non è possibile salvare dei progressi se il monitoraggio non è ancora stato attivato.", ToastLevel.Warning);
                return;
            }

            var today = DateTime.Now;
            Model.Reports.Add(new Report()
            {
                TimeEfficiently = new DateTime(today.Year, today.Month, today.Day, Hours, Minutes, Seconds),
                TimeInactive = new DateTime(today.Year, today.Month, today.Day, LockHours, LockMinutes, LockSeconds),
                ReportTime = today
            });
            await DataModelService.AddOrUpdateAsync(nameof(PomodoroTimer), Model);

            if (!TimerService.InProgress)
            {
                elapsedTime = TimeSpan.Zero;
                lockTime = TimeSpan.Zero;
            }
            ToastService.Show("Salvataggio effettuato con successo.", ToastLevel.Success);
        }

        private void SaveAlert()
        {
            if (Model.Minutes <= 0)
            {
                Model.Minutes = 0;
                ToastService.Show("Per configurare un alert è obbligatorio inserire un numero di minuti valido.", ToastLevel.Warning);
                return;
            }
        }

        private async Task DeleteReportAsync(Report report)
        {
            try
            {
                Model.Reports.Remove(report);
                await DataModelService.AddOrUpdateAsync(nameof(PomodoroTimer), Model);
                ToastService.Show("Report eliminato con successo.", ToastLevel.Success, 5);
            }
            catch
            {
                ToastService.Show("Si è verificato un errore durante la cancellazione del report; riprova.", ToastLevel.Error, 5);
            }
            finally
            {
                StateHasChanged();
            }
        }

        protected override async Task OnModelChangedAsync()
        {
            await DataModelService.AddOrUpdateAsync(nameof(PomodoroTimer), Model);
            base.OnModelChangedAsync();
        }

        private void OnLockTimeEvent()
        {
            lockTime = TimerService.TotalLockTime;
            InvokeAsync(StateHasChanged);
        }

        private void OnTickEvent()
        {
            elapsedTime = TimerService.ElapsedTime;
            InvokeAsync(StateHasChanged);
        }
    }
}
