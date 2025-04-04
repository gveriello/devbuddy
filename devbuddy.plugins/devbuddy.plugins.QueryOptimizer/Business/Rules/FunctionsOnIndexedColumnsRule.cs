using System.Text.RegularExpressions;

namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class FunctionsOnIndexedColumnsRule : IOptimizationRule
    {
        public string Id => "function_on_indexed";
        public string Description => "Funzioni applicate a colonne indicizzate";
        public SeverityLevel Severity => SeverityLevel.Medium;

        private static readonly string[] FunctionPatterns = new string[]
        {
            @"UPPER\s*\(\s*([^\(\)]+)\s*\)",
            @"LOWER\s*\(\s*([^\(\)]+)\s*\)",
            @"SUBSTRING\s*\(\s*([^\(\)]+)\s*,",
            @"CONCAT\s*\(\s*([^\(\)]+)\s*,",
            @"TRIM\s*\(\s*([^\(\)]+)\s*\)",
            @"REPLACE\s*\(\s*([^\(\)]+)\s*,",
            @"CAST\s*\(\s*([^\(\)]+)\s* AS",
            @"CONVERT\s*\(\s*\w+\s*,\s*([^\(\)]+)\s*\)"
        };

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            // Questa è una semplificazione - in un'implementazione reale dovremmo analizzare l'AST
            // Per questo esempio, utilizziamo l'analisi della query originale
            if (schema != null && !string.IsNullOrEmpty(ast.OriginalQuery))
            {
                // Cerchiamo pattern di funzioni nella clausola WHERE
                if (ast.Where != null)
                {
                    foreach (var condition in ast.Where.Conditions)
                    {
                        foreach (var pattern in FunctionPatterns)
                        {
                            var matches = Regex.Matches(condition.LeftExpression, pattern, RegexOptions.IgnoreCase);
                            foreach (Match match in matches)
                            {
                                string columnExpression = match.Groups[1].Value.Trim();

                                if (IsIndexed(columnExpression, schema))
                                {
                                    issues.Add(new QueryIssue
                                    {
                                        Id = Id,
                                        Description = Description,
                                        Message = $"Funzione applicata alla colonna indicizzata {columnExpression} nel filtro. Questo può impedire l'uso dell'indice.",
                                        Severity = Severity,
                                        Location = "0:0"
                                    });
                                }
                            }
                        }
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
            var match = Regex.Match(issue.Message, @"colonna indicizzata ([^\s]+) nel filtro");
            if (!match.Success)
                return null;

            string columnExpression = match.Groups[1].Value;

            // Cerca la funzione applicata
            string functionName = "";
            string originalCondition = "";

            foreach (var pattern in FunctionPatterns)
            {
                var funcMatch = Regex.Match(ast.OriginalQuery, pattern.Replace(@"([^\(\)]+)", Regex.Escape(columnExpression)), RegexOptions.IgnoreCase);
                if (funcMatch.Success)
                {
                    functionName = pattern.Split('\\')[0];
                    int startPos = Math.Max(0, funcMatch.Index - 20);
                    int endPos = Math.Min(ast.OriginalQuery.Length, funcMatch.Index + funcMatch.Length + 20);
                    originalCondition = ast.OriginalQuery.Substring(startPos, endPos - startPos);
                    break;
                }
            }

            if (string.IsNullOrEmpty(functionName))
            {
                return null;
            }

            // Formato previsto: table.column o column
            string tableName = "";
            string columnName = columnExpression;

            var parts = columnExpression.Split('.');
            if (parts.Length == 2)
            {
                tableName = parts[0];
                columnName = parts[1];
            }

            string suggestion = "-- Non è possibile generare una soluzione ottimale automaticamente, ma ecco alcuni suggerimenti:\n\n";
            suggestion += $"-- 1. Se possibile, rimuovere la funzione {functionName} da {columnExpression} nella WHERE\n";
            suggestion += $"-- 2. Mantenere il valore senza la funzione nella tabella, se possibile\n";

            if (!string.IsNullOrEmpty(tableName) && functionName.Equals("UPPER", StringComparison.OrdinalIgnoreCase) || functionName.Equals("LOWER", StringComparison.OrdinalIgnoreCase))
            {
                suggestion += $"-- 3. Su database supportati, creare un indice funzionale:\n";
                suggestion += $"-- CREATE INDEX idx_{tableName}_{columnName}_{functionName.ToLower()} ON {tableName}({functionName}({columnName}));\n";
            }

            return new OptimizationSuggestion
            {
                Id = "fix_function_on_indexed",
                Description = $"Evitare la funzione {functionName} sulla colonna indicizzata {columnExpression}",
                OptimizedQuery = suggestion,
                Explanation = $"L'applicazione della funzione {functionName} sulla colonna indicizzata {columnExpression} impedisce l'uso efficiente dell'indice. Utilizzare la colonna direttamente o creare un indice funzionale può migliorare significativamente le performance.",
                EstimatedImprovement = 45.0
            };
        }
    }
}
