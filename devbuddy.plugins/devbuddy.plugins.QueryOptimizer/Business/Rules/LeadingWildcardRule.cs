using System.Text.RegularExpressions;

namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class LeadingWildcardRule : IOptimizationRule
    {
        public string Id => "leading_wildcard";
        public string Description => "LIKE con wildcard iniziale ('%xyz')";
        public SeverityLevel Severity => SeverityLevel.High;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            if (ast.Type == "SELECT" && ast.Where != null)
            {
                // Cerchiamo pattern LIKE '%xyz'
                foreach (var condition in ast.Where.Conditions)
                {
                    if (condition.Operator?.ToUpperInvariant() == "LIKE" &&
                        condition.RightExpression != null &&
                        condition.RightExpression.StartsWith("%"))
                    {
                        issues.Add(new QueryIssue
                        {
                            Id = Id,
                            Description = Description,
                            Message = $"Uso di LIKE con wildcard iniziale su {condition.LeftExpression}. Questo tipo di ricerca non può utilizzare indici standard.",
                            Severity = Severity,
                            Location = "0:0"
                        });
                    }
                }
            }

            return issues;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            // Estraiamo il nome della colonna dal messaggio
            var match = Regex.Match(issue.Message, @"wildcard iniziale su ([^\s\.]+)");
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

            string suggestion = "-- LIKE con wildcard iniziale ('%xyz') non può utilizzare indici standard. Opzioni:\n\n";

            // Suggerimenti in base al database
            if (schema?.DatabaseType == "sqlserver")
            {
                suggestion += $"-- 1. Utilizzare una ricerca full-text se disponibile:\n";
                suggestion += $"-- CREATE FULLTEXT INDEX ON {tableName}({columnName}) KEY INDEX PK_{tableName};\n";
                suggestion += $"-- Poi usare: WHERE CONTAINS({columnName}, 'parola*') invece di LIKE\n\n";
            }
            else if (schema?.DatabaseType == "postgresql")
            {
                suggestion += $"-- 1. Utilizzare indici GIN con estensione pg_trgm:\n";
                suggestion += $"-- CREATE EXTENSION pg_trgm;\n";
                suggestion += $"-- CREATE INDEX trgm_idx_{tableName}_{columnName} ON {tableName} USING GIN ({columnName} gin_trgm_ops);\n\n";
            }
            else if (schema?.DatabaseType == "mysql")
            {
                suggestion += $"-- 1. Utilizzare indici FULLTEXT:\n";
                suggestion += $"-- ALTER TABLE {tableName} ADD FULLTEXT INDEX ft_idx_{columnName} ({columnName});\n";
                suggestion += $"-- Poi usare: WHERE MATCH({columnName}) AGAINST('parola*' IN BOOLEAN MODE)\n\n";
            }

            suggestion += "-- 2. Se possibile, ristrutturare la query per evitare wildcard iniziali\n";
            suggestion += "-- 3. Considerare l'uso di viste materializzate o tabelle di ricerca pre-calcolate\n";

            return new OptimizationSuggestion
            {
                Id = "fix_leading_wildcard",
                Description = "Ottimizzare ricerca con LIKE '%xyz'",
                OptimizedQuery = suggestion,
                Explanation = "Le ricerche con wildcard iniziale non possono utilizzare indici B-tree standard, causando scansioni complete della tabella. Le soluzioni dipendono dal database utilizzato e includono indici full-text, indici trigram, o ristrutturazione della query.",
                EstimatedImprovement = 70.0
            };
        }
    }
}
