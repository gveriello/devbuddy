namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class ComplexGroupByRule : IOptimizationRule
    {
        public string Id => "complex_group_by";
        public string Description => "GROUP BY su troppe colonne o con alta cardinalità";
        public SeverityLevel Severity => SeverityLevel.Medium;

        // Soglia oltre la quale consideriamo il GROUP BY complesso
        private const int GROUP_BY_COLUMN_THRESHOLD = 3;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            if (ast.Type == "SELECT" && ast.GroupBy != null && ast.GroupBy.Columns.Count > GROUP_BY_COLUMN_THRESHOLD)
            {
                issues.Add(new QueryIssue
                {
                    Id = Id,
                    Description = Description,
                    Message = $"GROUP BY su {ast.GroupBy.Columns.Count} colonne. Un numero elevato di colonne in GROUP BY può ridurre le performance.",
                    Severity = Severity,
                    Location = "0:0"
                });
            }

            return issues;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            string suggestion = "-- GROUP BY su molte colonne può ridurre le performance. Suggerimenti:\n\n";

            suggestion += "-- 1. Verificare se tutte le colonne nel GROUP BY sono necessarie\n";
            suggestion += "-- 2. Considerare l'uso di CTE per pre-aggregare i dati:\n";
            suggestion += "-- WITH pre_aggregated AS (\n";
            suggestion += "--     SELECT col1, col2, SUM(value) as sum_value\n";
            suggestion += "--     FROM table\n";
            suggestion += "--     GROUP BY col1, col2\n";
            suggestion += "-- )\n";
            suggestion += "-- SELECT col1, SUM(sum_value)\n";
            suggestion += "-- FROM pre_aggregated\n";
            suggestion += "-- GROUP BY col1\n\n";

            suggestion += "-- 3. Creare indici compositi che includano tutte le colonne di GROUP BY:\n";
            if (ast.GroupBy != null && ast.GroupBy.Columns.Count > 0 && ast.FromTables.Count > 0)
            {
                string tableName = ast.FromTables[0].TableName;
                string indexColumns = string.Join(", ", ast.GroupBy.Columns);

                suggestion += $"-- CREATE INDEX idx_{tableName}_groupby ON {tableName}({indexColumns});\n\n";
            }

            suggestion += "-- 4. Considerare l'uso di viste materializzate per query aggregate frequenti\n";

            return new OptimizationSuggestion
            {
                Id = "optimize_complex_group_by",
                Description = "Ottimizzare GROUP BY complessi",
                OptimizedQuery = suggestion,
                Explanation = "GROUP BY su molte colonne può causare operazioni di ordinamento e hash costose. Ridurre il numero di colonne, utilizzare indici compositi, o pre-aggregare i dati può migliorare significativamente le performance.",
                EstimatedImprovement = 35.0
            };
        }
    }
}
