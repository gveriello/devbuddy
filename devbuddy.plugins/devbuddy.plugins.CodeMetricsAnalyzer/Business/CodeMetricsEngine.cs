using devbuddy.plugins.CodeMetricsAnalyzer.Models;
using devbuddy.plugins.CodeMetricsAnalyzer.Business.Analyzers;
using devbuddy.plugins.CodeMetricsAnalyzer.Business.Rules;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.RegularExpressions;
using devbuddy.plugins.CodeMetricsAnalyzer.Business.Analyzers.Base;
using devbuddy.plugins.CodeMetricsAnalyzer.Business.Rules.Base;

namespace devbuddy.plugins.CodeMetricsAnalyzer.Business
{
    public class CodeMetricsEngine
    {
        private readonly List<RuleBase> _rules;
        private readonly Dictionary<string, LanguageAnalyzerBase> _analyzers;

        public CodeMetricsEngine()
        {
            // Initialize rules
            _rules = new List<RuleBase>
            {
                new ComplexityRule(),
                new MethodLengthRule(),
                //new ClassCouplingRule(),
                //new DuplicationRule()
            };

            // Initialize language analyzers
            _analyzers = new Dictionary<string, LanguageAnalyzerBase>
            {
                { ".cs", new CSharpAnalyzer() },
                //{ ".js", new JavaScriptAnalyzer() },
                //{ ".ts", new TypeScriptAnalyzer() },
                //{ ".py", new PythonAnalyzer() }
            };
        }

        public async Task<ProjectMetrics> AnalyzeProjectAsync(string projectPath)
        {
            var projectMetrics = new ProjectMetrics
            {
                ProjectName = Path.GetFileName(projectPath),
                ProjectPath = projectPath,
                AnalysisDate = DateTime.Now
            };

            var files = await ScanProjectForFilesAsync(projectPath);

            foreach (var file in files)
            {
                var fileMetrics = await AnalyzeFileAsync(file);
                if (fileMetrics != null)
                {
                    projectMetrics.Files.Add(fileMetrics);

                    // Aggregate issues from file
                    projectMetrics.Issues.AddRange(fileMetrics.Issues);

                    // Aggregate counts
                    projectMetrics.TotalLines += fileMetrics.TotalLines;
                    projectMetrics.TotalCodeLines += fileMetrics.CodeLines;
                    projectMetrics.TotalCommentLines += fileMetrics.CommentLines;
                }
            }

            // Calculate project level metrics
            if (projectMetrics.TotalCodeLines > 0)
            {
                projectMetrics.CommentsRatio = (double)projectMetrics.TotalCommentLines / projectMetrics.TotalCodeLines * 100;
            }

            // Calculate complexity averages
            if (projectMetrics.Files.Count > 0)
            {
                projectMetrics.AverageComplexity = projectMetrics.Files.Average(f => f.CyclomaticComplexity);
                projectMetrics.MaintainabilityIndex = projectMetrics.Files.Average(f => f.MaintainabilityIndex);
            }

            // Count issues by severity
            projectMetrics.CriticalIssues = projectMetrics.Issues.Count(i => i.Severity == MetricsSeverity.Critical);
            projectMetrics.HighIssues = projectMetrics.Issues.Count(i => i.Severity == MetricsSeverity.High);
            projectMetrics.MediumIssues = projectMetrics.Issues.Count(i => i.Severity == MetricsSeverity.Medium);
            projectMetrics.LowIssues = projectMetrics.Issues.Count(i => i.Severity == MetricsSeverity.Low);

            return projectMetrics;
        }

        private async Task<List<string>> ScanProjectForFilesAsync(string projectPath)
        {
            var supportedExtensions = _analyzers.Keys.ToList();
            var files = new List<string>();

            // Recursively scan directory for files with supported extensions
            foreach (var file in Directory.GetFiles(projectPath, "*.*", SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(file).ToLower();
                if (supportedExtensions.Contains(extension))
                {
                    files.Add(file);
                }
            }

            return files;
        }

        private async Task<FileMetrics> AnalyzeFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            if (!_analyzers.TryGetValue(extension, out var analyzer))
                return null;

            var fileContent = await File.ReadAllTextAsync(filePath);

            // Create basic file metrics
            var fileMetrics = new FileMetrics
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Language = extension.TrimStart('.'),
                TotalLines = fileContent.Split('\n').Length
            };

            // Use the appropriate analyzer to analyze the file
            await analyzer.AnalyzeFileAsync(fileContent, fileMetrics);

            // Apply all rules to find issues
            foreach (var rule in _rules)
            {
                var issues = rule.Analyze(fileMetrics);
                fileMetrics.Issues.AddRange(issues);
            }

            return fileMetrics;
        }

        public string GenerateReport(ProjectMetrics projectMetrics)
        {
            return new ReportGenerator().GenerateHtmlReport(projectMetrics);
        }
    }
}