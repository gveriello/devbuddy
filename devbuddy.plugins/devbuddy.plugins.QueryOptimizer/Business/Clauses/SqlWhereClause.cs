using devbuddy.plugins.QueryOptimizer.Business.Conditions;

namespace devbuddy.plugins.QueryOptimizer.Business.Clauses
{
    public class SqlWhereClause
    {
        public List<SqlCondition> Conditions { get; set; } = [];
    }
}
