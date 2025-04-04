using devbuddy.plugins.QueryOptimizer.Business.Conditions;

namespace devbuddy.plugins.QueryOptimizer.Business.Basic
{
    public class SqlJoin
    {
        public string Type { get; set; } // INNER, LEFT, RIGHT, FULL
        public string TableName { get; set; }
        public string Alias { get; set; }
        public SqlJoinCondition Condition { get; set; }
    }
}
