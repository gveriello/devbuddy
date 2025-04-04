namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class TooManyJoinsRule : IOptimizationRule
    {
        public string Id => "too_many_joins";
        public string Description => "Troppe tabelle unite in una singola query";
        public SeverityLevel Severity => SeverityLevel.Medium;

        // Soglia oltre la quale consideriamo troppi join
        private const int JOIN_THRESHOLD = 5;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            if (ast.Type == "SELECT" && ast.Joins.Count >= JOIN_THRESHOLD)
            {
                issues.Add(new QueryIssue
                {
                    Id = Id,
                    Description = Description,
                    Message = $"La query unisce {ast.Joins.Count + 1} tabelle. Un numero elevato di join può ridurre le performance.",
                    Severity = Severity,
                    Location = "0:0"
                });
            }

            return issues;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            string suggestion = "-- Suggerimenti per ottimizzare query con molti join:\n\n";

            suggestion += "-- 1. Suddividere la query in più query separate o utilizzare CTE (Common Table Expressions)\n";
            suggestion += "-- 2. Considerare la creazione di viste materializzate per aggregazioni frequenti\n";
            suggestion += "-- 3. Verificare che tutti i join siano effettivamente necessari\n";
            suggestion += "-- 4. Assicurarsi che tutte le colonne di join siano correttamente indicizzate\n";
            suggestion += "-- 5. Considerare la denormalizzazione strategica per query ad alta frequenza\n\n";

            suggestion += "-- Esempio di approccio con CTE:\n";
            suggestion += "WITH cte_name AS (\n";
            suggestion += "    SELECT ... FROM table1 JOIN table2 ON ...\n";
            suggestion += "),\n";
            suggestion += "cte_name2 AS (\n";
            suggestion += "    SELECT ... FROM cte_name JOIN table3 ON ...\n";
            suggestion += ")\n";
            suggestion += "SELECT ... FROM cte_name2 JOIN table4 ON ...\n";

            return new OptimizationSuggestion
            {
                Id = "optimize_many_joins",
                Description = "Ottimizzare query con molti join",
                OptimizedQuery = suggestion,
                Explanation = "Le query con molti join possono causare problemi di performance a causa della complessità dell'ottimizzazione. Suddividere la query, utilizzare CTE, o creare viste materializzate può migliorare significativamente le performance.",
                EstimatedImprovement = 40.0
            };
        }
    }
}
