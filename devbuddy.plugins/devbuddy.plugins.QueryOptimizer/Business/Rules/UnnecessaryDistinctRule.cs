namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class UnnecessaryDistinctRule : IOptimizationRule
    {
        public string Id => "unnecessary_distinct";
        public string Description => "Uso non necessario di DISTINCT";
        public SeverityLevel Severity => SeverityLevel.Low;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            if (ast.Type == "SELECT" && ast.HasDistinct)
            {
                // Verifichiamo se la query include già JOIN su chiavi primarie
                // o se include GROUP BY, rendendo potenzialmente ridondante il DISTINCT
                bool hasJoinOnPrimaryKey = false;
                bool hasGroupBy = ast.GroupBy != null && ast.GroupBy.Columns.Count > 0;

                foreach (var join in ast.Joins)
                {
                    if (join.Condition != null &&
                        IsColumnPrimaryKey(join.Condition.LeftExpression, schema) &&
                        IsColumnPrimaryKey(join.Condition.RightExpression, schema))
                    {
                        hasJoinOnPrimaryKey = true;
                        break;
                    }
                }

                if (hasJoinOnPrimaryKey || hasGroupBy)
                {
                    issues.Add(new QueryIssue
                    {
                        Id = Id,
                        Description = Description,
                        Message = hasGroupBy
                            ? "Uso di DISTINCT con GROUP BY. DISTINCT potrebbe essere ridondante."
                            : "Uso di DISTINCT con JOIN su chiavi primarie. DISTINCT potrebbe essere ridondante.",
                        Severity = Severity,
                        Location = "0:0"
                    });
                }
            }

            return issues;
        }

        private bool IsColumnPrimaryKey(string columnExpression, DatabaseSchema schema)
        {
            if (schema == null || string.IsNullOrEmpty(columnExpression))
                return false;

            // Formato previsto: table.column
            var parts = columnExpression.Split('.');
            if (parts.Length != 2)
                return false;

            string tableName = parts[0];
            string columnName = parts[1];

            if (schema.Tables.ContainsKey(tableName))
            {
                var table = schema.Tables[tableName];

                foreach (var index in table.Indices)
                {
                    if (index.IsPrimary && index.Columns.Contains(columnName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            // Creiamo una copia della query senza DISTINCT
            string optimizedQuery = ast.OriginalQuery.Replace("SELECT DISTINCT", "SELECT").Replace("select distinct", "select");

            return new OptimizationSuggestion
            {
                Id = "remove_unnecessary_distinct",
                Description = "Rimuovere DISTINCT non necessario",
                OptimizedQuery = optimizedQuery,
                Explanation = "L'operatore DISTINCT può essere costoso perché richiede ordinamento o hash. " +
                              "In questa query, DISTINCT sembra ridondante poiché la struttura della query (JOIN su chiavi primarie o presenza di GROUP BY) " +
                              "garantisce già l'unicità dei risultati. Rimuovere DISTINCT può migliorare le performance.",
                EstimatedImprovement = 20.0
            };
        }
    }
}
