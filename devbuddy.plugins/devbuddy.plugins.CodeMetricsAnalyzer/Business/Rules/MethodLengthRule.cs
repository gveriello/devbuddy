using devbuddy.plugins.CodeMetricsAnalyzer.Business.Rules.Base;
using devbuddy.plugins.CodeMetricsAnalyzer.Models;

namespace devbuddy.plugins.CodeMetricsAnalyzer.Business.Rules
{
    public class MethodLengthRule : RuleBase
    {
        public override string Id => "MLEN001";
        public override string Title => "Metodo troppo lungo";
        public override string Description => "Un metodo contiene troppe righe di codice, rendendo difficile la manutenzione.";
        public override MetricsSeverity Severity => MetricsSeverity.Medium;
        public override string DocumentationLink => "https://refactoring.guru/extract-method";

        private const int MethodLengthThresholdMedium = 30;
        private const int MethodLengthThresholdHigh = 50;
        private const int MethodLengthThresholdCritical = 100;

        public override List<IssueDetail> Analyze(FileMetrics file)
        {
            var issues = new List<IssueDetail>();

            foreach (var classMetric in file.Classes)
            {
                foreach (var methodMetric in classMetric.Methods)
                {
                    if (methodMetric.LineCount >= MethodLengthThresholdCritical)
                    {
                        var issue = CreateIssue(
                            file.FilePath,
                            classMetric.ClassName,
                            methodMetric.MethodName,
                            methodMetric.StartLine,
                            0,
                            suggestion: $"Il metodo '{methodMetric.MethodName}' è estremamente lungo ({methodMetric.LineCount} righe). " +
                                        $"Metodi così lunghi sono difficili da comprendere e mantenere. " +
                                        $"Considera di suddividere questo metodo in più metodi più piccoli con responsabilità ben definite."
                        );
                        issue.Severity = MetricsSeverity.Critical;
                        issues.Add(issue);
                    }
                    else if (methodMetric.LineCount >= MethodLengthThresholdHigh)
                    {
                        var issue = CreateIssue(
                            file.FilePath,
                            classMetric.ClassName,
                            methodMetric.MethodName,
                            methodMetric.StartLine,
                            0,
                            suggestion: $"Il metodo '{methodMetric.MethodName}' è molto lungo ({methodMetric.LineCount} righe). " +
                                        $"Metodi lunghi tendono ad avere più responsabilità e sono più difficili da testare. " +
                                        $"Valuta la possibilità di estrarre parti logiche in metodi separati."
                        );
                        issue.Severity = MetricsSeverity.High;
                        issues.Add(issue);
                    }
                    else if (methodMetric.LineCount >= MethodLengthThresholdMedium)
                    {
                        var issue = CreateIssue(
                            file.FilePath,
                            classMetric.ClassName,
                            methodMetric.MethodName,
                            methodMetric.StartLine,
                            0,
                            suggestion: $"Il metodo '{methodMetric.MethodName}' è abbastanza lungo ({methodMetric.LineCount} righe). " +
                                       $"Considera di rivedere se può essere semplificato o suddiviso in metodi più piccoli."
                        );
                        issues.Add(issue);
                    }
                }
            }

            return issues;
        }
    }
}