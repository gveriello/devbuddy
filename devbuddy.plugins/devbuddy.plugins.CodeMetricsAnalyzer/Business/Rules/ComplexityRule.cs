using devbuddy.plugins.CodeMetricsAnalyzer.Business.Rules.Base;
using devbuddy.plugins.CodeMetricsAnalyzer.Models;

namespace devbuddy.plugins.CodeMetricsAnalyzer.Business.Rules
{
    public class ComplexityRule : RuleBase
    {
        public override string Id => "CMPLX001";
        public override string Title => "Eccessiva complessità ciclomatica";
        public override string Description => "La complessità ciclomatica di un metodo supera la soglia consigliata.";
        public override MetricsSeverity Severity => MetricsSeverity.High;
        public override string DocumentationLink => "https://en.wikipedia.org/wiki/Cyclomatic_complexity";

        private const int ComplexityThresholdHigh = 15;
        private const int ComplexityThresholdCritical = 25;

        public override List<IssueDetail> Analyze(FileMetrics file)
        {
            var issues = new List<IssueDetail>();

            foreach (var classMetric in file.Classes)
            {
                foreach (var methodMetric in classMetric.Methods)
                {
                    if (methodMetric.CyclomaticComplexity >= ComplexityThresholdCritical)
                    {
                        var issue = CreateIssue(
                            file.FilePath,
                            classMetric.ClassName,
                            methodMetric.MethodName,
                            methodMetric.StartLine,
                            0,
                            suggestion: $"Il metodo '{methodMetric.MethodName}' ha una complessità ciclomatica di {methodMetric.CyclomaticComplexity}, " +
                                        $"che è ben al di sopra della soglia critica di {ComplexityThresholdCritical}. " +
                                        $"Considera di suddividere il metodo in più metodi più piccoli e con responsabilità singole."
                        );

                        // Override severity for critical issues
                        issue.Severity = MetricsSeverity.Critical;
                        issues.Add(issue);
                    }
                    else if (methodMetric.CyclomaticComplexity >= ComplexityThresholdHigh)
                    {
                        var issue = CreateIssue(
                            file.FilePath,
                            classMetric.ClassName,
                            methodMetric.MethodName,
                            methodMetric.StartLine,
                            0,
                            suggestion: $"Il metodo '{methodMetric.MethodName}' ha una complessità ciclomatica di {methodMetric.CyclomaticComplexity}, " +
                                        $"che è al di sopra della soglia raccomandata di {ComplexityThresholdHigh}. " +
                                        $"Considera di semplificare il metodo e di estrarre parti della logica in metodi separati."
                        );
                        issues.Add(issue);
                    }
                }
            }

            return issues;
        }
    }
}