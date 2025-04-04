namespace devbuddy.plugins.QueryOptimizer.Business.Plan
{
    public class ExecutionPlan
    {
        public ExecutionPlanNode RootNode { get; set; }
        public double TotalCost { get; set; }
        public double EstimatedRows { get; set; }
    }
}
