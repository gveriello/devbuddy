namespace devbuddy.common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IconAttribute(string icon) : Attribute
    {
        public string Icon { get; set; } = icon;
    }
}
