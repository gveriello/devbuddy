using devbuddy.common.Applications;

namespace devbuddy.plugins.CronExpression.Models
{
    public class CronExpressionDataModel : CustomDataModelBase
    {
        public List<SavedExpression> SavedExpressions { get; set; } = [];
        public string? CurrentExpression { get; set; }
    }

    public class SavedExpression
    {
        public string Name { get; set; }
        public string Expression { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
