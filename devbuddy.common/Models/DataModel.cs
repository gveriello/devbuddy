using devbuddy.common.Applications;

namespace devbuddy.common.Models
{
    public sealed class DataModel
    {
        public List<NavItem> SidebarItems { get; set; } = [];
        public SettingsDataModel Settings { get; set; } = new();
        public Dictionary<string, string> ApplicationsDataModels { get; set; } = [];
        public Dictionary<string, List<TaskBase>> Tasks { get; set; } = [];
    }
}
