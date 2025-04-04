using devbuddy.plugins.QueryOptimizer.Business.Models.Enum;
using devbuddy.plugins.QueryOptimizer.Business.Schemas;

namespace devbuddy.plugins.QueryOptimizer.Business.Contracts
{
    public interface IOptimizationRule
    {
        string Id { get; }
        string Description { get; }
        SeverityLevel Severity { get; }
        List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema);
        OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue);
    }
}
