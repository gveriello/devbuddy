using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.Services;
using devbuddy.plugins.CodeMetricsAnalyzer.Business;
using devbuddy.plugins.CodeMetricsAnalyzer.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace devbuddy.plugins.CodeMetricsAnalyzer
{
    public partial class Index : AppComponentBase<CodeMetricsDataModel>
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private ToastService ToastService { get; set; }

        private string ProjectPath { get; set; } = string.Empty;
        private ProjectMetrics CurrentProject { get; set; }
        private bool IsAnalyzing { get; set; } = false;
        private double AnalysisProgress { get; set; } = 0;
        private string AnalysisStatusMessage { get; set; } = "Analyzing files...";

        private FileMetrics SelectedFile { get; set; }
        private ClassMetrics SelectedClass { get; set; }
        private MethodMetrics SelectedMethod { get; set; }

        private List<ProjectMetrics> RecentProjects { get; set; } = new();
        private List<ProjectMetrics> HistoryProjects { get; set; } = new();
        private List<MetricsHistoryEntry> MetricsHistory { get; set; } = new();
        private bool HasHistory => Model.Projects.Count > 0;

        private List<(string Id, string Title, string Description, MetricsSeverity Severity, bool IsEnabled)> AvailableRules { get; set; } = new()
        {
            ("CMPLX001", "Excessive cyclomatic complexity", "Methods with high cyclomatic complexity are difficult to understand and test.", MetricsSeverity.High, true),
            ("MLEN001", "Method too long", "Long methods often try to do too much and violate the Single Responsibility Principle.", MetricsSeverity.Medium, true),
            ("CLSCPL001", "High class coupling", "Classes with too many dependencies are difficult to modify and test.", MetricsSeverity.Medium, true),
            ("CLSCOH001", "Low class cohesion", "Classes where methods don't share instance variables may need to be split.", MetricsSeverity.Medium, true),
            ("DUPCD001", "Duplicated code", "Repeated code increases maintenance effort and risk of bugs.", MetricsSeverity.High, true),
            ("CMTRT001", "Low comment ratio", "Inadequate comments make code harder to understand and maintain.", MetricsSeverity.Low, true)
        };

        private long SelectedHistoryProjectId { get; set; }
        private ComparisonResult ComparisonResult { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Model = DataModelService.ValueByKey<CodeMetricsDataModel>(nameof(CodeMetricsAnalyzer));

            if (Model.Projects.Count > 0)
            {
                RecentProjects = Model.Projects
                    .OrderByDescending(p => p.AnalysisDate)
                    .Take(5)
                    .ToList();

                if (Model.MetricsHistory.Count > 0)
                {
                    string projectKey = Model.Projects.Last().ProjectName;
                    if (Model.MetricsHistory.TryGetValue(projectKey, out var history))
                    {
                        MetricsHistory = history;
                    }
                }
            }

            await base.OnInitializedAsync();
        }

        private async Task BrowseFolder()
        {
            try
            {
                var folderPath = await JSRuntime.InvokeAsync<string>("window.showDirectoryPicker");
                if (!string.IsNullOrEmpty(folderPath))
                {
                    ProjectPath = folderPath;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                ToastService.Show($"Error selecting folder: {ex.Message}", ToastLevel.Error);
            }
        }

        private async Task AnalyzeProject()
        {
            if (string.IsNullOrWhiteSpace(ProjectPath))
            {
                ToastService.Show("Please enter a project path", ToastLevel.Warning);
                return;
            }

            if (!Directory.Exists(ProjectPath))
            {
                ToastService.Show("The specified directory does not exist", ToastLevel.Error);
                return;
            }

            try
            {
                IsAnalyzing = true;
                AnalysisProgress = 0;
                AnalysisStatusMessage = "Initializing analysis...";
                StateHasChanged();

                // Create background task to show progress
                var progressTask = Task.Run(async () => {
                    for (int i = 1; i <= 100 && IsAnalyzing; i++)
                    {
                        await Task.Delay(100); // Adjust for actual analysis time

                        if (i % 10 == 0)
                        {
                            // Update UI with progress
                            await InvokeAsync(() => {
                                AnalysisProgress = i;

                                if (i < 30)
                                    AnalysisStatusMessage = "Scanning project files...";
                                else if (i < 60)
                                    AnalysisStatusMessage = "Analyzing code complexity...";
                                else if (i < 80)
                                    AnalysisStatusMessage = "Detecting issues...";
                                else
                                    AnalysisStatusMessage = "Calculating metrics...";

                                StateHasChanged();
                            });
                        }
                    }
                });

                // Create and run the analyzer
                var analyzer = new CodeMetricsEngine();
                CurrentProject = await analyzer.AnalyzeProjectAsync(ProjectPath);

                // Add to recent projects
                if (!Model.RecentProjects.Contains(ProjectPath))
                {
                    Model.RecentProjects.Add(ProjectPath);
                    if (Model.RecentProjects.Count > 10)
                    {
                        Model.RecentProjects.RemoveAt(0);
                    }
                }

                // Create history entry
                var historyEntry = new MetricsHistoryEntry
                {
                    Date = DateTime.Now,
                    ComplexityScore = CurrentProject.AverageComplexity,
                    MaintainabilityIndex = CurrentProject.MaintainabilityIndex,
                    TotalIssues = CurrentProject.Issues.Count
                };

                string projectKey = CurrentProject.ProjectName;
                if (!Model.MetricsHistory.ContainsKey(projectKey))
                {
                    Model.MetricsHistory[projectKey] = new List<MetricsHistoryEntry>();
                }

                Model.MetricsHistory[projectKey].Add(historyEntry);
                MetricsHistory = Model.MetricsHistory[projectKey];

                // Update the model
                await DataModelService.AddOrUpdateAsync(nameof(CodeMetricsAnalyzer), Model);

                // Update UI
                AnalysisProgress = 100;
                AnalysisStatusMessage = "Analysis complete!";
                RecentProjects = Model.Projects
                    .OrderByDescending(p => p.AnalysisDate)
                    .Take(5)
                    .ToList();

                ToastService.Show("Project analysis complete", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show($"Error analyzing project: {ex.Message}", ToastLevel.Error);
            }
            finally
            {
                IsAnalyzing = false;
                StateHasChanged();
            }
        }

        private void ClearResults()
        {
            CurrentProject = null;
            SelectedFile = null;
            SelectedClass = null;
            SelectedMethod = null;
            StateHasChanged();
        }

        private async Task ExportReport()
        {
            if (CurrentProject == null)
                return;

            try
            {
                var reportGenerator = new ReportGenerator();
                string reportHtml = reportGenerator.GenerateHtmlReport(CurrentProject);

                // Generate filename based on project name and date
                string filename = $"{CurrentProject.ProjectName}-metrics-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.html";

                // Use JS to trigger download
                await JSRuntime.InvokeVoidAsync("saveAsFile", filename, reportHtml);

                ToastService.Show("Report exported successfully", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show($"Error exporting report: {ex.Message}", ToastLevel.Error);
            }
        }

        private async Task SaveToHistory()
        {
            if (CurrentProject == null)
                return;

            try
            {
                // Add to projects history
                Model.Projects.Add(CurrentProject);

                // Limit history size
                while (Model.Projects.Count > 50)
                {
                    Model.Projects.RemoveAt(0);
                }

                // Update model
                await DataModelService.AddOrUpdateAsync(nameof(CodeMetricsAnalyzer), Model);

                // Update UI
                HistoryProjects = Model.Projects
                    .OrderByDescending(p => p.AnalysisDate)
                    .ToList();

                ToastService.Show("Project saved to history", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show($"Error saving to history: {ex.Message}", ToastLevel.Error);
            }
        }

        private async Task CompareWithHistory()
        {
            // Get history for comparison
            HistoryProjects = Model.Projects
                .Where(p => p.ProjectName == CurrentProject?.ProjectName)
                .OrderByDescending(p => p.AnalysisDate)
                .ToList();

            if (HistoryProjects.Count > 0)
            {
                SelectedHistoryProjectId = HistoryProjects[0].AnalysisDate.Ticks;
            }

            // Show comparison modal
            await JSRuntime.InvokeVoidAsync("showModal", "historyCompareModal");
        }

        private async Task RunComparison()
        {
            if (CurrentProject == null || SelectedHistoryProjectId == 0)
                return;

            try
            {
                // Find the selected project from history
                var historyProject = HistoryProjects.FirstOrDefault(p => p.AnalysisDate.Ticks == SelectedHistoryProjectId);
                if (historyProject == null)
                    return;

                // Compare metrics
                double complexityChange = CurrentProject.AverageComplexity - historyProject.AverageComplexity;
                double maintainabilityChange = CurrentProject.MaintainabilityIndex - historyProject.MaintainabilityIndex;
                int issuesChange = CurrentProject.Issues.Count - historyProject.Issues.Count;

                // Determine overall status
                string overallStatus = "info";
                string overallMessage = "The codebase has seen minimal changes since the previous analysis.";

                if (complexityChange > 2 || maintainabilityChange < -5 || issuesChange > 5)
                {
                    overallStatus = "danger";
                    overallMessage = "The codebase quality has significantly decreased since the previous analysis.";
                }
                else if (complexityChange < -2 || maintainabilityChange > 5 || issuesChange < -5)
                {
                    overallStatus = "success";
                    overallMessage = "The codebase quality has significantly improved since the previous analysis.";
                }
                else if (complexityChange > 0.5 || maintainabilityChange < -2 || issuesChange > 0)
                {
                    overallStatus = "warning";
                    overallMessage = "The codebase quality has slightly decreased since the previous analysis.";
                }
                else if (complexityChange < -0.5 || maintainabilityChange > 2 || issuesChange < 0)
                {
                    overallStatus = "success";
                    overallMessage = "The codebase quality has improved since the previous analysis.";
                }

                // Create comparison result
                ComparisonResult = new ComparisonResult
                {
                    ComplexityChange = complexityChange,
                    MaintainabilityChange = maintainabilityChange,
                    IssuesChange = issuesChange,
                    OverallStatus = overallStatus,
                    OverallMessage = overallMessage
                };

                StateHasChanged();
            }
            catch (Exception ex)
            {
                ToastService.Show($"Error running comparison: {ex.Message}", ToastLevel.Error);
            }
        }

        private async Task ConfigureRules()
        {
            await JSRuntime.InvokeVoidAsync("showModal", "rulesConfigModal");
        }

        private void ToggleRule(string ruleId)
        {
            var rule = AvailableRules.FirstOrDefault(r => r.Id == ruleId);
            if (rule != default)
            {
                int index = AvailableRules.IndexOf(rule);
                var newRule = (rule.Id, rule.Title, rule.Description, rule.Severity, !rule.IsEnabled);
                AvailableRules.RemoveAt(index);
                AvailableRules.Insert(index, newRule);
            }
        }

        private async Task SaveRulesConfig()
        {
            // In a real implementation, this would persist rule configuration
            // For this demo, just display a success message
            ToastService.Show("Rules configuration saved", ToastLevel.Success);
        }

        private void ShowFileDetails(FileMetrics file)
        {
            SelectedFile = file;
            SelectedClass = null;
            SelectedMethod = null;
            StateHasChanged();
        }

        private void LoadProject(ProjectMetrics project)
        {
            CurrentProject = project;
            SelectedFile = null;
            SelectedClass = null;
            SelectedMethod = null;

            // Update metrics history
            string projectKey = project.ProjectName;
            if (Model.MetricsHistory.TryGetValue(projectKey, out var history))
            {
                MetricsHistory = history;
            }
            else
            {
                MetricsHistory = new List<MetricsHistoryEntry>();
            }

            StateHasChanged();
        }

        private string GetSeverityBadgeClass(MetricsSeverity severity)
        {
            return severity switch
            {
                MetricsSeverity.Critical => "danger",
                MetricsSeverity.High => "warning",
                MetricsSeverity.Medium => "info",
                MetricsSeverity.Low => "secondary",
                _ => "secondary"
            };
        }

        private string GetComparisonClass(double change)
        {
            if (change > 0)
                return "danger";
            else if (change < 0)
                return "success";
            else
                return "secondary";
        }
    }

    public class ComparisonResult
    {
        public double ComplexityChange { get; set; }
        public double MaintainabilityChange { get; set; }
        public int IssuesChange { get; set; }
        public string OverallStatus { get; set; }
        public string OverallMessage { get; set; }
    }
}