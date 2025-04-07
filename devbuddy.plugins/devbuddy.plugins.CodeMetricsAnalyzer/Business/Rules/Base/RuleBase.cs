using devbuddy.plugins.CodeMetricsAnalyzer.Models;

namespace devbuddy.plugins.CodeMetricsAnalyzer.Business.Rules.Base
{
    public abstract class RuleBase
    {
        public abstract string Id { get; }
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract MetricsSeverity Severity { get; }
        public abstract string DocumentationLink { get; }

        // Implementazione specifica della regola
        public abstract List<IssueDetail> Analyze(FileMetrics file);

        // Helper method per generare un'istanza di IssueDetail
        protected IssueDetail CreateIssue(
            string filePath,
            string className,
            string methodName,
            int line,
            int column,
            string codeSnippet = null,
            string suggestion = null)
        {
            return new IssueDetail
            {
                Id = Id,
                Title = Title,
                Description = Description,
                Severity = Severity,
                FilePath = filePath,
                ClassName = className,
                MethodName = methodName,
                Line = line,
                Column = column,
                CodeSnippet = codeSnippet,
                Suggestion = suggestion,
                DocumentationLink = DocumentationLink
            };
        }
    }
}
