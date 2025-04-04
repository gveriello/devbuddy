using devbuddy.plugins.QueryOptimizer.Business.Contracts;
using devbuddy.plugins.QueryOptimizer.Business.Models.Enum;
using devbuddy.plugins.QueryOptimizer.Business.Schemas;

namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class MissingWhereRule : IOptimizationRule
    {
        public string Id => "missing_where";
        public string Description => "Query senza clausola WHERE su tabelle grandi";
        public SeverityLevel Severity => SeverityLevel.Critical;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = [];

            if (ast.Type == "SELECT" && (ast.Where == null || ast.Where.Conditions.Count == 0))
            {
                // Idealmente controlleremmo se le tabelle sono considerate "grandi"
                // basandoci su statistiche dal DB o metadata
                issues.Add(new QueryIssue
                {
                    Id = Id,
                    Description = Description,
                    Message = "Query senza clausola WHERE. Potrebbe causare scansioni di tabella complete.",
                    Severity = Severity,
                    Location = "0:0"
                });
            }

            return issues;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            return new OptimizationSuggestion
            {
                Id = "add_where_condition",
                Description = "Aggiungere una clausola WHERE",
                OptimizedQuery = ast.Type + " ... WHERE <condizione>",
                Explanation = "Aggiungere una clausola WHERE appropriata per limitare i risultati solo ai dati necessari.",
                EstimatedImprovement = 80.0
            };
        }
    }
}
