using System.Text.RegularExpressions;

namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class MultiColumnOrRule : IOptimizationRule
    {
        public string Id => "multi_column_or";
        public string Description => "Operazioni OR su colonne diverse";
        public SeverityLevel Severity => SeverityLevel.Medium;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            if (ast.Type == "SELECT" && ast.Where != null)
            {
                // Cerchiamo condizioni OR
                var distinctColumns = new HashSet<string>();
                bool hasMultiColumnOr = false;

                foreach (var condition in ast.Where.Conditions)
                {
                    if (condition.LogicalOperator?.ToUpperInvariant() == "OR")
                    {
                        // Aggiungiamo la colonna a sinistra
                        string leftColumn = ExtractColumn(condition.LeftExpression);
                        if (!string.IsNullOrEmpty(leftColumn))
                        {
                            distinctColumns.Add(leftColumn);
                        }

                        // Controlliamo anche eventuali condizioni annidate
                        foreach (var nested in condition.NestedConditions)
                        {
                            string nestedLeftColumn = ExtractColumn(nested.LeftExpression);
                            if (!string.IsNullOrEmpty(nestedLeftColumn))
                            {
                                distinctColumns.Add(nestedLeftColumn);
                            }
                        }
                    }
                }

                if (distinctColumns.Count > 1)
                {
                    hasMultiColumnOr = true;
                }

                if (hasMultiColumnOr)
                {
                    issues.Add(new QueryIssue
                    {
                        Id = Id,
                        Description = Description,
                        Message = $"La query utilizza operatori OR su colonne diverse ({string.Join(", ", distinctColumns)}). Questo può impedire l'uso efficiente degli indici.",
                        Severity = Severity,
                        Location = "0:0"
                    });
                }
            }

            return issues;
        }

        private string ExtractColumn(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return null;

            // Formato previsto: table.column o column
            var parts = expression.Split('.');
            if (parts.Length == 2)
            {
                return parts[0] + "." + parts[1]; // Tabella.colonna
            }
            else if (parts.Length == 1)
            {
                return parts[0]; // Solo colonna
            }

            return null;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            // Estraiamo le colonne dal messaggio
            var match = Regex.Match(issue.Message, @"colonne diverse \(([^\)]+)\)");
            if (!match.Success)
                return null;

            string columnsStr = match.Groups[1].Value;
            var columns = columnsStr.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            string suggestion = "-- OR su colonne diverse può impedire l'uso efficiente degli indici. Alternative:\n\n";

            suggestion += "-- 1. Riscrivere utilizzando UNION ALL (spesso più efficiente):\n";
            suggestion += "-- Invece di: SELECT ... WHERE col1 = 'val1' OR col2 = 'val2'\n";
            suggestion += "-- Usare:     SELECT ... WHERE col1 = 'val1' \n";
            suggestion += "--            UNION ALL\n";
            suggestion += "--            SELECT ... WHERE col2 = 'val2' AND col1 <> 'val1'\n\n";

            suggestion += "-- 2. Creare indici compositi se le combinazioni di colonne sono fisse:\n";

            if (columns.Length >= 2 && !string.IsNullOrEmpty(columns[0]) && !string.IsNullOrEmpty(columns[1]))
            {
                string col1 = columns[0];
                string col2 = columns[1];

                // Estraiamo la tabella dalla prima colonna se possibile
                string tableName = "";
                var parts = col1.Split('.');
                if (parts.Length == 2)
                {
                    tableName = parts[0];
                    col1 = parts[1];
                }

                // Estraiamo il nome della seconda colonna
                parts = col2.Split('.');
                if (parts.Length == 2)
                {
                    col2 = parts[1];
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    suggestion += $"-- CREATE INDEX idx_{tableName}_{col1}_{col2} ON {tableName}({col1}, {col2});\n";
                }
            }

            suggestion += "\n-- 3. Valutare la creazione di indici separati e lasciare che l'ottimizzatore scelga:\n";
            foreach (var column in columns)
            {
                string col = column;
                string tableName = "";

                var parts = column.Split('.');
                if (parts.Length == 2)
                {
                    tableName = parts[0];
                    col = parts[1];
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    suggestion += $"-- CREATE INDEX idx_{tableName}_{col} ON {tableName}({col});\n";
                }
            }

            return new OptimizationSuggestion
            {
                Id = "fix_multi_column_or",
                Description = "Ottimizzare operazioni OR su colonne diverse",
                OptimizedQuery = suggestion,
                Explanation = "Gli operatori OR su colonne diverse spesso impediscono l'uso efficiente degli indici. Riscrivere la query utilizzando UNION ALL o creare indici appropriati può migliorare significativamente le performance.",
                EstimatedImprovement = 55.0
            };
        }
    }
}
