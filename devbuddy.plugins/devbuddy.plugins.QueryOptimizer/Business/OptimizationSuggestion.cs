namespace devbuddy.plugins.QueryOptimizer.Business
{
    public class OptimizationSuggestion
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string OptimizedQuery { get; set; }
        public string Explanation { get; set; }
        public double EstimatedImprovement { get; set; }
    }
}
