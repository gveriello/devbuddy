namespace devbuddy.plugins.QueryOptimizer.Business.Plan
{
    public class ExecutionPlanNode
    {
        public string Operation { get; set; }
        public string ObjectName { get; set; }
        public double Cost { get; set; }
        public double Rows { get; set; }
        public List<ExecutionPlanNode> Children { get; set; } = new List<ExecutionPlanNode>();
    }
}
