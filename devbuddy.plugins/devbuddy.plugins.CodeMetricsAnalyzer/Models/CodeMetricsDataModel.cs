using devbuddy.common.Applications;

namespace devbuddy.plugins.CodeMetricsAnalyzer.Models
{
    public class CodeMetricsDataModel : CustomDataModelBase
    {
        public List<ProjectMetrics> Projects { get; set; } = [];
        public Dictionary<string, List<MetricsHistoryEntry>> MetricsHistory { get; set; } = [];
        public List<string> RecentProjects { get; set; } = [];
    }

    public class MetricsHistoryEntry
    {
        public DateTime Date { get; set; }
        public double ComplexityScore { get; set; }
        public double MaintainabilityIndex { get; set; }
        public int TotalIssues { get; set; }
    }

    public class ProjectMetrics
    {
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<FileMetrics> Files { get; set; } = [];
        public List<IssueDetail> Issues { get; set; } = [];

        // Aggregated metrics
        public int TotalLines { get; set; }
        public int TotalCodeLines { get; set; }
        public int TotalCommentLines { get; set; }
        public double CommentsRatio { get; set; }
        public double AverageComplexity { get; set; }
        public double MaintainabilityIndex { get; set; }
        public int DuplicatedLines { get; set; }
        public double DuplicationRatio { get; set; }
        public int CriticalIssues { get; set; }
        public int HighIssues { get; set; }
        public int MediumIssues { get; set; }
        public int LowIssues { get; set; }
    }

    public class FileMetrics
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Language { get; set; }
        public int TotalLines { get; set; }
        public int CodeLines { get; set; }
        public int CommentLines { get; set; }
        public double CommentsRatio { get; set; }
        public double CyclomaticComplexity { get; set; }
        public double MaintainabilityIndex { get; set; }
        public List<ClassMetrics> Classes { get; set; } = [];
        public List<IssueDetail> Issues { get; set; } = [];
    }

    public class ClassMetrics
    {
        public string ClassName { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int LineCount { get; set; }
        public double CouplingDegree { get; set; }
        public double CohesionMetric { get; set; }
        public double MaintainabilityIndex { get; set; }
        public List<MethodMetrics> Methods { get; set; } = [];
        public List<IssueDetail> Issues { get; set; } = [];
    }

    public class MethodMetrics
    {
        public string MethodName { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int LineCount { get; set; }
        public int ParameterCount { get; set; }
        public double CyclomaticComplexity { get; set; }
        public double CognitiveComplexity { get; set; }
        public double MaintainabilityIndex { get; set; }
        public List<IssueDetail> Issues { get; set; } = [];
    }

    public class IssueDetail
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public MetricsSeverity Severity { get; set; }
        public string FilePath { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string CodeSnippet { get; set; }
        public string Suggestion { get; set; }
        public string DocumentationLink { get; set; }
    }

    public enum MetricsSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
}
