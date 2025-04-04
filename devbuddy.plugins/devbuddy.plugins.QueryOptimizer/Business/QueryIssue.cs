using devbuddy.plugins.QueryOptimizer.Business.Models.Enum;

namespace devbuddy.plugins.QueryOptimizer.Business
{
    public class QueryIssue
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public SeverityLevel Severity { get; set; }
        public string Location { get; set; }
    }
}
