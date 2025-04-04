using System.Text.RegularExpressions;

namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class InefficientSubqueryRule : IOptimizationRule
    {
        public string Id => "inefficient_subquery";
        public string Description => "Subquery che potrebbero essere riscritte come JOIN";
        public SeverityLevel Severity => SeverityLevel.Medium;

        private static readonly string[] SubqueryPatterns = new string[]
        {
            @"WHERE\s+\w+\s+IN\s*\(\s*SELECT",
            @"WHERE\s+\w+\s+=\s*\(\s*SELECT",
            @"WHERE\s+EXISTS\s*\(\s*SELECT",
            @"FROM\s+\(\s*SELECT.*?\)\s+AS"
        };

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            // In una implementazione reale, analizzeremmo l'AST per le subquery
            // Per questo esempio, utilizziamo espressioni regolari sulla query originale
            if (!string.IsNullOrEmpty(ast.OriginalQuery))
            {
                foreach (var pattern in SubqueryPatterns)
                {
                    var matches = Regex.Matches(ast.OriginalQuery, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    foreach (Match match in matches)
                    {
                        issues.Add(new QueryIssue
                        {
                            Id = Id,
                            Description = Description,
                            Message = $"Subquery rilevata che potrebbe essere riscritta come JOIN per una migliore performance.",
                            Severity = Severity,
                            Location = "0:0"
                        });

                        // Evitiamo di segnalare più volte lo stesso problema
                        break;
                    }

                    if (issues.Count > 0)
                        break;
                }
            }

            return issues;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            string subqueryType = "";

            // Determiniamo il tipo di subquery
            if (Regex.IsMatch(ast.OriginalQuery, @"WHERE\s+\w+\s+IN\s*\(\s*SELECT", RegexOptions.IgnoreCase))
            {
                subqueryType = "IN";
            }
            else if (Regex.IsMatch(ast.OriginalQuery, @"WHERE\s+\w+\s+=\s*\(\s*SELECT", RegexOptions.IgnoreCase))
            {
                subqueryType = "=";
            }
            else if (Regex.IsMatch(ast.OriginalQuery, @"WHERE\s+EXISTS\s*\(\s*SELECT", RegexOptions.IgnoreCase))
            {
                subqueryType = "EXISTS";
            }
            else if (Regex.IsMatch(ast.OriginalQuery, @"FROM\s+\(\s*SELECT.*?\)\s+AS", RegexOptions.IgnoreCase))
            {
                subqueryType = "FROM";
            }

            string suggestion = "-- Le subquery spesso possono essere riscritte come JOIN per migliori performance.\n\n";

            if (subqueryType == "IN" || subqueryType == "=")
            {
                suggestion += "-- Invece di:\n";
                suggestion += "-- SELECT columns FROM table1 WHERE column IN (SELECT column FROM table2 WHERE conditions)\n\n";
                suggestion += "-- Usare:\n";
                suggestion += "-- SELECT DISTINCT table1.columns\n";
                suggestion += "-- FROM table1\n";
                suggestion += "-- INNER JOIN table2 ON table1.column = table2.column AND conditions\n";
            }
            else if (subqueryType == "EXISTS")
            {
                suggestion += "-- Invece di:\n";
                suggestion += "-- SELECT columns FROM table1 WHERE EXISTS (SELECT 1 FROM table2 WHERE table2.col = table1.col)\n\n";
                suggestion += "-- Usare:\n";
                suggestion += "-- SELECT DISTINCT table1.columns\n";
                suggestion += "-- FROM table1\n";
                suggestion += "-- INNER JOIN table2 ON table2.col = table1.col\n";
            }
            else if (subqueryType == "FROM")
            {
                suggestion += "-- Invece di:\n";
                suggestion += "-- SELECT columns FROM (SELECT cols FROM table WHERE conditions) AS subquery\n\n";
                suggestion += "-- Considerare:\n";
                suggestion += "-- 1. Utilizzare CTE per maggiore leggibilità e potenzialmente migliori performance:\n";
                suggestion += "-- WITH cte_name AS (\n";
                suggestion += "--     SELECT cols FROM table WHERE conditions\n";
                suggestion += "-- )\n";
                suggestion += "-- SELECT columns FROM cte_name\n\n";
                suggestion += "-- 2. Valutare se la subquery può essere incorporata nella query principale\n";
            }
            else
            {
                suggestion += "-- Suggerimento generico: riscrivere la subquery come JOIN o CTE può spesso migliorare le performance\n";
            }

            suggestion += "\n-- Nota: L'effettivo miglioramento dipende dalla cardinalità delle tabelle e dalla struttura degli indici.";

            return new OptimizationSuggestion
            {
                Id = "convert_subquery_to_join",
                Description = "Convertire subquery in JOIN",
                OptimizedQuery = suggestion,
                Explanation = "Le subquery nidificate possono causare problemi di performance poiché il database potrebbe dover eseguire la subquery per ogni riga della query esterna. Riscriverle come JOIN può permettere all'ottimizzatore di scegliere piani di esecuzione più efficienti.",
                EstimatedImprovement = 50.0
            };
        }
    }
}
