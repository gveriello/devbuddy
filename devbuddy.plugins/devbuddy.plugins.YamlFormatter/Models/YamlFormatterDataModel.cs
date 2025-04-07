using devbuddy.common.Applications;

namespace devbuddy.plugins.YamlFormatter.Models
{
    public class YamlFormatterDataModel : CustomDataModelBase
    {
        public List<SavedYaml> SavedYamls { get; set; } = [];
        public string? CurrentYaml { get; set; }
    }

    public class SavedYaml
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}