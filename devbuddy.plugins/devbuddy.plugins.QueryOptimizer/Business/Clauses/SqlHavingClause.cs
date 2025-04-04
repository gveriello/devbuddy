using devbuddy.plugins.QueryOptimizer.Business.Conditions;

namespace devbuddy.plugins.QueryOptimizer.Business.Clauses
{
    public class SqlHavingClause
    {
        public List<SqlCondition> Conditions { get; set; } = [];
    }
}
