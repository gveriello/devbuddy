using System.Text.Json;
using devbuddy.plugins.QueryOptimizer.Business.Contracts;
using devbuddy.plugins.QueryOptimizer.Business.Models.Enum;
using devbuddy.plugins.QueryOptimizer.Business.Plan;
using devbuddy.plugins.QueryOptimizer.Business.Rules;
using devbuddy.plugins.QueryOptimizer.Business.Schemas;
using Microsoft.JSInterop;

namespace devbuddy.plugins.QueryOptimizer.Business
{
    public class Optimizer
    {
        private readonly IJSRuntime jsRuntime;
        private readonly string databaseType;
        private readonly OptimizationRulesManager rulesManager;

        public Optimizer(IJSRuntime jsRuntime, string databaseType = "sqlserver")
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