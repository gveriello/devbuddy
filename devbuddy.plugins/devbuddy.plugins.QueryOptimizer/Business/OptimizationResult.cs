using devbuddy.plugins.QueryOptimizer.Business.Plan;

namespace devbuddy.plugins.QueryOptimizer.Business
{
    public class OptimizationResult
    {
        public string OriginalQuery { get; set; }
        public string OptimizedQuery { get; set; }
        public List<QueryIssue> Issues { get; set; } = new List<QueryIssue>();
        public List<OptimizationSuggestion> Suggestions { get; set; } = new List<OptimizationSuggestion>();
        public ExecutionPlan OriginalPlan { get; set; }
        public ExecutionPlan OptimizedPlan { get; set; }
        public double EstimatedImprovement { get; set; }
        public string ConfidenceLevel { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
