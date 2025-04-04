using devbuddy.plugins.QueryOptimizer.Business.Basic;
using devbuddy.plugins.QueryOptimizer.Business.Clauses;

namespace devbuddy.plugins.QueryOptimizer.Business
{
    public class SqlAst
    {
        public string Type { get; set; } // SELECT, INSERT, UPDATE, DELETE
        public List<SqlSelectColumn> Columns { get; set; } = new List<SqlSelectColumn>();
        public List<SqlFromTable> FromTables { get; set; } = new List<SqlFromTable>();
        public List<SqlJoin> Joins { get; set; } = new List<SqlJoin>();
        public SqlWhereClause Where { get; set; }
        public SqlGroupByClause GroupBy { get; set; }
        public SqlHavingClause Having { get; set; }
        public SqlOrderByClause OrderBy { get; set; }
        public SqlLimitClause Limit { get; set; }
        public string OriginalQuery { get; internal set; }
        public bool HasDistinct { get; internal set; }
    }
}
