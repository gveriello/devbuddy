namespace devbuddy.plugins.CronConverter.Models
{
    public class CronScheduleResult
    {
        public DateTime DateTime { get; set; }
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
