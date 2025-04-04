using System.Text.RegularExpressions;
using devbuddy.plugins.QueryOptimizer.Business.Contracts;
using devbuddy.plugins.QueryOptimizer.Business.Models.Enum;
using devbuddy.plugins.QueryOptimizer.Business.Schemas;

namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class NonIndexedWhereColumnsRule : IOptimizationRule
    {
        public string Id => "non_indexed_where";
        public string Description => "Colonne in WHERE prive di indici appropriati";
        public SeverityLevel Severity => SeverityLevel.High;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = [];

            if (ast.Type == "SELECT" && ast.Where != null && ast.Where.Conditions.Count > 0 && schema != null)
            {
                foreach (var condition in ast.Where.Conditions)
                {
                    // Analizziamo solo condizioni semplici con colonne a sinistra
                    if (!string.IsNullOrEmpty(condition.LeftExpression) && !IsIndexed(condition.LeftExpression, schema))
                    {
                        issues.Add(new QueryIssue
                        {
                            Id = Id,
                            Description = Description,
                            Message = $"La colonna {condition.LeftExpression} nella clausola WHERE non è indicizzata.",
                            Severity = Severity,
                            Location = "0:0"
                        });
                    }
                }
            }

            return issues;
        }

        private bool IsIndexed(string columnExpression, DatabaseSchema schema)
        {
            // Formato previsto: table.column o column
            string tableName = "";
            string columnName = columnExpression;

            var parts = columnExpression.Split('.');
            if (parts.Length == 2)
            {
                tableName = parts[0];
                columnName = parts[1];
            }

            // Cerchiamo la tabella nello schema
            foreach (var tableEntry in schema.Tables)
            {
                string currentTable = tableEntry.Key;
                var table = tableEntry.Value;

                // Se è stata specificata una tabella, verifico solo quella tabella
                if (!string.IsNullOrEmpty(tableName) && currentTable != tableName)
                {
                    continue;
                }

                // Verifico se la colonna è indicizzata
                foreach (var index in table.Indices)
                {
                    if (index.Columns.Contains(columnName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            // Estraiamo il nome della colonna dal messaggio
            var match = Regex.Match(issue.Message, @"La colonna ([^\s]+) nella clausola WHERE");
            if (!match.Success)
                return null;

            string columnExpression = match.Groups[1].Value;

            // Formato previsto: table.column o column
            string tableName = "";
            string columnName = columnExpression;

            var parts = columnExpression.Split('.');
            if (parts.Length == 2)
            {
                tableName = parts[0];
                columnName = parts[1];
            }
            else
            {
                // Se non è specificata la tabella, cerchiamo di determinarla dall'AST
                if (ast.FromTables.Count == 1)
                {
                    tableName = ast.FromTables[0].TableName;
                }
            }

            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }

            // Generiamo il suggerimento per creare un indice
            string createIndexSql = $"CREATE INDEX idx_{tableName}_{columnName} ON {tableName}({columnName});";

            return new OptimizationSuggestion
            {
                Id = "create_index_where",
                Description = $"Creare un indice su {tableName}.{columnName}",
                OptimizedQuery = createIndexSql,
                Explanation = $"Creare un indice sulla colonna {columnName} migliorerà significativamente le performance di questa query, permettendo al database di evitare scansioni complete della tabella.",
                EstimatedImprovement = 60.0
            };
        }
    }
}
