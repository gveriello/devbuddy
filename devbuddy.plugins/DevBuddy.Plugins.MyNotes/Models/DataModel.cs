using devbuddy.common.Applications;

namespace devbuddy.Plugins.MyNotes.Models
{
    public class DataModel : CustomDataModelBase
    {
        public Dictionary<int, Note> Rows { get; set; } = [];
        public static int ItemsPerPage { get; set; } = 5;
    }
}
