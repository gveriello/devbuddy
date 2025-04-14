using System.Timers;
using devbuddy.common.Applications;
using Timer = System.Timers.Timer;

namespace devbuddy.common.Services
{
    public class TasksExecuterService
    {
        private readonly DataModelService _dataModelService;
        private List<TaskExecute> _tasksToExecute;
        public TasksExecuterService(DataModelService dataModelService)
        {
            this._dataModelService = dataModelService;
            LoadTasksToExecute();
            StartAsync();
        }

        private Task StartAsync()
        {
            _ = Task.Run(() =>
            {
                var timerService = new Timer(1000);
                timerService.Elapsed += TimerService_Elapsed;
                timerService.Start();
            });
            return Task.CompletedTask;
        }

        private void TimerService_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            Task.Run(() =>
            {
                var tasks = _tasksToExecute.Where(task => task.NextExecute == now)?.ToList();

                if (tasks?.Count == 0)
                    return;


            });
        }

        private void LoadTasksToExecute()
        {
            //_tasksToExecute = _dataModelService.GetTasks().Cast<TaskExecute>().ToList();
            _tasksToExecute.ForEach(task => task.NextExecute = DateTime.Now.AddMinutes(task.Minutes));
        }
    }

    public class TaskExecute : TaskBase
    {
        public DateTime? LastExecute { get; set; }
        public DateTime? NextExecute { get; set; }
    }
}
