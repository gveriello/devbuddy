namespace devbuddy.common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class SoundAttribute(string name) : Attribute
    {
        public string Name { get; set; } = name;
    }
}
