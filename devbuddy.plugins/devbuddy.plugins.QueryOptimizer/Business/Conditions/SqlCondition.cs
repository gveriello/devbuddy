namespace devbuddy.plugins.QueryOptimizer.Business.Conditions
{
    public class SqlCondition
    {
        public string LeftExpression { get; set; }
        public string Operator { get; set; }
        public string RightExpression { get; set; }
        public List<SqlCondition> NestedConditions { get; set; } = new List<SqlCondition>();
        public string LogicalOperator { get; set; } // AND, OR
    }
}
