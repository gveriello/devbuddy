namespace devbuddy.common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ModuleKeyAttribute(string key) : Attribute
    {
        public string Key { get; set; } = key;
    }
}
