namespace devbuddy.common.Applications
{
    public abstract class CustomDataModelBase
    {
        public DateTime? LastUsed { get; set; } = DateTime.Now;
        public string InstanceName => GetType().Name;
    }
}
