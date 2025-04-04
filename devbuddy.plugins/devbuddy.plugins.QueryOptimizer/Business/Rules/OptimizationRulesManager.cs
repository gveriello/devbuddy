namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class OptimizationRulesManager
    {
        private readonly List<IOptimizationRule> _rules;

        public OptimizationRulesManager()
        {
            // Inizializza tutte le regole disponibili
            _rules =
            [
                new SelectStarRule(),
                new MissingWhereRule(),
                new NonIndexedWhereColumnsRule(),
                new InefficientJoinRule(),
                new FunctionsOnIndexedColumnsRule(),
                new LeadingWildcardRule(),
                new TooManyJoinsRule(),
                new MultiColumnOrRule(),
                new ComplexGroupByRule(),
                new UnnecessaryDistinctRule(),
                new InefficientSubqueryRule()
            ];
        }

        public IEnumerable<IOptimizationRule> GetAllRules()
        {
            return _rules;
        }

        public IEnumerable<IOptimizationRule> GetRulesByCategory(string category)
        {
            // Implementa la logica per filtrare le regole per categoria
            return _rules;
        }

        public IEnumerable<IOptimizationRule> GetRulesBySeverity(SeverityLevel severity)
        {
            return _rules.Where(r => r.Severity == severity);
        }

        public IOptimizationRule GetRuleById(string id)
        {
            return _rules.FirstOrDefault(r => r.Id == id);
        }

        public void RegisterCustomRule(IOptimizationRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            // Verifica che non esista già una regola con lo stesso ID
            if (_rules.Any(r => r.Id == rule.Id))
                throw new InvalidOperationException($"Una regola con ID '{rule.Id}' è già registrata.");

            _rules.Add(rule);
        }

        public List<QueryIssue> AnalyzeQuery(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> allIssues = [];

            foreach (var rule in _rules)
            {
                try
                {
                    var issues = rule.Check(ast, schema);
                    if (issues != null && issues.Count > 0)
                    {
                        allIssues.AddRange(issues);
                    }
                }
                catch (Exception ex)
                {
                    // Log dell'errore e continuazione con la regola successiva
                    Console.WriteLine($"Errore nell'applicazione della regola {rule.Id}: {ex.Message}");
                }
            }

            return allIssues;
        }

        public List<OptimizationSuggestion> GenerateSuggestions(SqlAst ast, DatabaseSchema schema, List<QueryIssue> issues)
        {
            List<OptimizationSuggestion> suggestions = [];

            foreach (var issue in issues)
            {
                var rule = _rules.FirstOrDefault(r => r.Id == issue.Id);
                if (rule != null)
                {
                    try
                    {
                        var suggestion = rule.GenerateSuggestion(ast, schema, issue);
                        if (suggestion != null)
                        {
                            suggestions.Add(suggestion);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log dell'errore e continuazione con l'issue successiva
                        Console.WriteLine($"Errore nella generazione del suggerimento per l'issue {issue.Id}: {ex.Message}");
                    }
                }
            }

            return suggestions;
        }
    }
}
