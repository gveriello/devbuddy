using devbuddy.common.Applications;

namespace devbuddy.Plugins.PomodoroTimer.Models
{
    public class PomodoroTimerDataModel : CustomDataModelBase
    {
        public int Minutes { get; set; }
        public string? Text { get; set; }
        public List<Report> Reports { get; set; } = [];
    }

    public class Report
    {
        public DateTime TimeEfficiently { get; set; }
        public DateTime TimeInactive { get; set; }
        public DateTime ReportTime { get; set; }
    }
}
