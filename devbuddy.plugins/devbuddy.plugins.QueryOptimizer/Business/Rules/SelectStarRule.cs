namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class SelectStarRule : IOptimizationRule
    {
        public string Id => "select_star";
        public string Description => "Evitare SELECT * e specificare solo le colonne necessarie";
        public SeverityLevel Severity => SeverityLevel.Medium;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            if (ast.Type == "SELECT")
            {
                bool hasSelectStar = ast.Columns.Any(c => c.IsAsterisk && string.IsNullOrEmpty(c.TableName));

                if (hasSelectStar)
                {
                    issues.Add(new QueryIssue
                    {
                        Id = Id,
                        Description = Description,
                        Message = "L'uso di SELECT * può causare performance ridotte. Specifica solo le colonne necessarie.",
                        Severity = Severity,
                        Location = "0:0"
                    });
                }
            }

            return issues;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            if (schema == null || ast.FromTables.Count == 0)
            {
                return null;
            }

            List<string> explicitColumns = [];

            // Per ogni tabella nella FROM clause o nelle JOIN, aggiungiamo tutte le colonne disponibili
            foreach (var table in ast.FromTables)
            {
                string tableName = table.TableName;
                string tableAlias = !string.IsNullOrEmpty(table.Alias) ? table.Alias : tableName;

                if (schema.Tables.ContainsKey(tableName))
                {
                    foreach (var column in schema.Tables[tableName].Columns)
                    {
                        explicitColumns.Add($"{tableAlias}.{column.Name}");
                    }
                }
            }

            foreach (var join in ast.Joins)
            {
                string tableName = join.TableName;
                string tableAlias = !string.IsNullOrEmpty(join.Alias) ? join.Alias : tableName;

                if (schema.Tables.ContainsKey(tableName))
                {
                    foreach (var column in schema.Tables[tableName].Columns)
                    {
                        explicitColumns.Add($"{tableAlias}.{column.Name}");
                    }
                }
            }

            // Creiamo una nuova query con le colonne esplicite
            string columnsString = string.Join(", ", explicitColumns);

            // Qui dovremmo ricostruire l'intera query, ma per semplicità facciamo solo una sostituzione
            // In una implementazione reale questo utilizzerebbe l'AST e un SQL generator
            string optimizedQuery = "SELECT " + columnsString + " FROM " + ast.FromTables[0].TableName;

            return new OptimizationSuggestion
            {
                Id = "explicit_columns",
                Description = "Selezione esplicita delle colonne",
                OptimizedQuery = optimizedQuery,
                Explanation = "Specificando esattamente le colonne necessarie, si riduce il traffico di rete e il carico sul database.",
                EstimatedImprovement = 15.0
            };
        }
    }

}
