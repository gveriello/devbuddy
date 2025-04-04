using devbuddy.plugins.QueryOptimizer.Business.Basic;

namespace devbuddy.plugins.QueryOptimizer.Business.Clauses
{
    public class SqlOrderByClause
    {
        public List<SqlOrderByColumn> Columns { get; set; } = [];
    }
}
