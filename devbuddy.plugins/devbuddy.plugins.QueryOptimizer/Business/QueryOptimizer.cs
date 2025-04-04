using System.Text.Json;
using devbuddy.plugins.QueryOptimizer.Business.Rules;
using Microsoft.JSInterop;

namespace devbuddy.plugins.QueryOptimizer.Business
{
    public enum SeverityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class QueryIssue
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public SeverityLevel Severity { get; set; }
        public string Location { get; set; }
    }

    public class OptimizationSuggestion
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string OptimizedQuery { get; set; }
        public string Explanation { get; set; }
        public double EstimatedImprovement { get; set; }
    }

    public class ExecutionPlanNode
    {
        public string Operation { get; set; }
        public string ObjectName { get; set; }
        public double Cost { get; set; }
        public double Rows { get; set; }
        public List<ExecutionPlanNode> Children { get; set; } = new List<ExecutionPlanNode>();
    }

    public class ExecutionPlan
    {
        public ExecutionPlanNode RootNode { get; set; }
        public double TotalCost { get; set; }
        public double EstimatedRows { get; set; }
    }

    public class OptimizationResult
    {
        public string OriginalQuery { get; set; }
        public string OptimizedQuery { get; set; }
        public List<QueryIssue> Issues { get; set; } = new List<QueryIssue>();
        public List<OptimizationSuggestion> Suggestions { get; set; } = new List<OptimizationSuggestion>();
        public ExecutionPlan OriginalPlan { get; set; }
        public ExecutionPlan OptimizedPlan { get; set; }
        public double EstimatedImprovement { get; set; }
        public string ConfidenceLevel { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface IOptimizationRule
    {
        string Id { get; }
        string Description { get; }
        SeverityLevel Severity { get; }
        List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema);
        OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue);
    }

    public class DatabaseSchema
    {
        public Dictionary<string, TableSchema> Tables { get; set; } = [];
        public string DatabaseType { get; set; }
    }

    public class TableSchema
    {
        public string Name { get; set; }
        public List<ColumnSchema> Columns { get; set; } = [];
        public List<IndexSchema> Indices { get; set; } = [];
    }

    public class ColumnSchema
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
    }

    public class IndexSchema
    {
        public string Name { get; set; }
        public List<string> Columns { get; set; } = new List<string>();
        public bool IsUnique { get; set; }
        public bool IsPrimary { get; set; }
    }

    // Questa classe rappresenta l'albero sintattico astratto di una query SQL
    // In una implementazione reale, questa sarebbe generata da un parser SQL
    public class SqlAst
    {
        public string Type { get; set; } // SELECT, INSERT, UPDATE, DELETE
        public List<SqlSelectColumn> Columns { get; set; } = new List<SqlSelectColumn>();
        public List<SqlFromTable> FromTables { get; set; } = new List<SqlFromTable>();
        public List<SqlJoin> Joins { get; set; } = new List<SqlJoin>();
        public SqlWhereClause Where { get; set; }
        public SqlGroupByClause GroupBy { get; set; }
        public SqlHavingClause Having { get; set; }
        public SqlOrderByClause OrderBy { get; set; }
        public SqlLimitClause Limit { get; set; }
        public string OriginalQuery { get; internal set; }
        public bool HasDistinct { get; internal set; }
    }

    public class SqlSelectColumn
    {
        public string Expression { get; set; }
        public string Alias { get; set; }
        public bool IsAsterisk { get; set; }
        public string TableName { get; set; } // Per colonne tipo "table.*"
    }

    public class SqlFromTable
    {
        public string TableName { get; set; }
        public string Alias { get; set; }
    }

    public class SqlJoin
    {
        public string Type { get; set; } // INNER, LEFT, RIGHT, FULL
        public string TableName { get; set; }
        public string Alias { get; set; }
        public SqlJoinCondition Condition { get; set; }
    }

    public class SqlJoinCondition
    {
        public string LeftExpression { get; set; }
        public string Operator { get; set; }
        public string RightExpression { get; set; }
    }

    public class SqlWhereClause
    {
        public List<SqlCondition> Conditions { get; set; } = new List<SqlCondition>();
    }

    public class SqlCondition
    {
        public string LeftExpression { get; set; }
        public string Operator { get; set; }
        public string RightExpression { get; set; }
        public List<SqlCondition> NestedConditions { get; set; } = new List<SqlCondition>();
        public string LogicalOperator { get; set; } // AND, OR
    }

    public class SqlGroupByClause
    {
        public List<string> Columns { get; set; } = new List<string>();
    }

    public class SqlHavingClause
    {
        public List<SqlCondition> Conditions { get; set; } = new List<SqlCondition>();
    }

    public class SqlOrderByClause
    {
        public List<SqlOrderByColumn> Columns { get; set; } = new List<SqlOrderByColumn>();
    }

    public class SqlOrderByColumn
    {
        public string Column { get; set; }
        public bool IsAscending { get; set; }
    }

    public class SqlLimitClause
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
    }

    public class QueryOptimizer
    {
        private readonly IJSRuntime jsRuntime;
        private readonly string databaseType;
        private readonly OptimizationRulesManager rulesManager;

        public QueryOptimizer(IJSRuntime jsRuntime, string databaseType = "sqlserver")
        {
            this.jsRuntime = jsRuntime;
            this.databaseType = databaseType;
            this.rulesManager = new OptimizationRulesManager();
        }

        public async Task<OptimizationResult> OptimizeAsync(string query, string schemaJson = null)
        {
            try
            {
                // Parse della query in AST
                SqlAst ast = await ParseSqlAsync(query);

                // Parse dello schema
                DatabaseSchema schema = null;
                if (!string.IsNullOrEmpty(schemaJson))
                {
                    schema = JsonSerializer.Deserialize<DatabaseSchema>(schemaJson);
                    schema.DatabaseType = databaseType;
                }

                // Analisi della query con tutte le regole
                List<QueryIssue> issues = rulesManager.AnalyzeQuery(ast, schema);

                // Generazione di suggerimenti per i problemi rilevati
                List<OptimizationSuggestion> suggestions = rulesManager.GenerateSuggestions(ast, schema, issues);

                // Generazione della query ottimizzata basata sui suggerimenti
                string optimizedQuery = GenerateOptimizedQuery(ast, suggestions);

                // Generazione dei piani di esecuzione
                ExecutionPlan originalPlan = GenerateExecutionPlan(query, false);
                ExecutionPlan optimizedPlan = GenerateExecutionPlan(optimizedQuery ?? query, true);

                // Calcolo del miglioramento stimato
                double improvement = CalculateImprovement(issues, originalPlan, optimizedPlan);

                return new OptimizationResult
                {
                    OriginalQuery = query,
                    OptimizedQuery = optimizedQuery,
                    Issues = issues,
                    Suggestions = suggestions,
                    OriginalPlan = originalPlan,
                    OptimizedPlan = optimizedPlan,
                    EstimatedImprovement = improvement,
                    ConfidenceLevel = issues.Count > 3 ? "Alta" : "Media"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    OriginalQuery = query,
                    HasError = true,
                    ErrorMessage = $"Errore nell'analisi della query: {ex.Message}"
                };
            }
        }

        private static async Task<SqlAst> ParseSqlAsync(string query)
        {
            var ast = new SqlAst
            {
                Type = "SELECT",
                OriginalQuery = query,
                HasDistinct = query.Contains("SELECT DISTINCT", StringComparison.InvariantCultureIgnoreCase)
            };

            // [Implementazione del parsing omessa per brevità]
            // Qui ci sarebbe un'analisi dettagliata della query per costruire l'AST completo

            return await Task.FromResult(ast);
        }

        private string GenerateOptimizedQuery(SqlAst ast, List<OptimizationSuggestion> suggestions)
        {
            if (suggestions == null || suggestions.Count == 0)
            {
                return string.Empty;
            }

            // Ordina i suggerimenti per stimato miglioramento (decrescente)
            suggestions.Sort((a, b) => b.EstimatedImprovement.CompareTo(a.EstimatedImprovement));

            // Per semplicità, prendiamo il primo suggerimento che ha una query ottimizzata
            foreach (var suggestion in suggestions)
            {
                if (!string.IsNullOrEmpty(suggestion.OptimizedQuery) &&
                    !suggestion.OptimizedQuery.StartsWith("--"))
                {
                    return suggestion.OptimizedQuery;
                }
            }

            return string.Empty;
        }

        private ExecutionPlan GenerateExecutionPlan(string query, bool isOptimized)
        {
            // [Implementazione del generatore di piani di esecuzione]
            // Questa sarebbe l'implementazione corretta che analizza la query e genera un piano adeguato

            var plan = new ExecutionPlan();

            // Codice di generazione del piano omesso per brevità

            return plan;
        }

        private double CalculateImprovement(List<QueryIssue> issues, ExecutionPlan originalPlan, ExecutionPlan optimizedPlan)
        {
            // Calcolo basato sulla differenza di costo tra i piani
            if (originalPlan != null && optimizedPlan != null && originalPlan.TotalCost > 0)
            {
                return Math.Min(95, (1 - (optimizedPlan.TotalCost / originalPlan.TotalCost)) * 100);
            }

            // Calcolo alternativo basato sulla gravità dei problemi
            var severityWeights = new Dictionary<SeverityLevel, int>
            {
                { SeverityLevel.Low, 1 },
                { SeverityLevel.Medium, 2 },
                { SeverityLevel.High, 3 },
                { SeverityLevel.Critical, 5 }
            }; 

            int totalWeight = 0;
            foreach (var issue in issues)
            {
                totalWeight += severityWeights[issue.Severity];
            }

            return Math.Min(95, totalWeight * 10);
        }

        // Metodi di utilità

        public IEnumerable<IOptimizationRule> GetAllRules()
        {
            return rulesManager.GetAllRules();
        }

        public void RegisterCustomRule(IOptimizationRule rule)
        {
            rulesManager.RegisterCustomRule(rule);
        }
    }
}