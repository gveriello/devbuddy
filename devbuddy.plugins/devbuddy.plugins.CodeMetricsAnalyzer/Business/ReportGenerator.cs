using devbuddy.plugins.CodeMetricsAnalyzer.Models;
using System.Text;

namespace devbuddy.plugins.CodeMetricsAnalyzer.Business
{
    public class ReportGenerator
    {
        public string GenerateHtmlReport(ProjectMetrics projectMetrics)
        {
            var sb = new StringBuilder();

            // Start HTML document
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("    <title>Code Metrics Report - " + projectMetrics.ProjectName + "</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }");
            sb.AppendLine("        h1, h2, h3 { color: #2c3e50; }");
            sb.AppendLine("        .header { display: flex; justify-content: space-between; align-items: center; }");
            sb.AppendLine("        .metrics-card { background: #f8f9fa; border-radius: 5px; padding: 15px; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .metrics-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 15px; }");
            sb.AppendLine("        .metric-item { background: white; padding: 10px; border-radius: 4px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .metric-value { font-size: 24px; font-weight: bold; }");
            sb.AppendLine("        .metric-name { font-size: 14px; color: #666; }");
            sb.AppendLine("        .good { color: #27ae60; }");
            sb.AppendLine("        .average { color: #f39c12; }");
            sb.AppendLine("        .poor { color: #e74c3c; }");
            sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
            sb.AppendLine("        th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }");
            sb.AppendLine("        th { background-color: #f2f2f2; }");
            sb.AppendLine("        tr:hover { background-color: #f5f5f5; }");
            sb.AppendLine("        .severity-Critical { color: #e74c3c; font-weight: bold; }");
            sb.AppendLine("        .severity-High { color: #e67e22; font-weight: bold; }");
            sb.AppendLine("        .severity-Medium { color: #f39c12; }");
            sb.AppendLine("        .severity-Low { color: #3498db; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header with project info
            sb.AppendLine("    <div class=\"header\">");
            sb.AppendLine($"        <h1>Code Metrics Report</h1>");
            sb.AppendLine($"        <div><strong>Date:</strong> {projectMetrics.AnalysisDate.ToString("yyyy-MM-dd HH:mm")}</div>");
            sb.AppendLine("    </div>");
            sb.AppendLine($"    <p><strong>Project:</strong> {projectMetrics.ProjectName}</p>");
            sb.AppendLine($"    <p><strong>Path:</strong> {projectMetrics.ProjectPath}</p>");

            // Summary metrics
            sb.AppendLine("    <div class=\"metrics-card\">");
            sb.AppendLine("        <h2>Summary Metrics</h2>");
            sb.AppendLine("        <div class=\"metrics-grid\">");

            // Lines of code metrics
            AppendMetricItem(sb, "Lines of Code", projectMetrics.TotalCodeLines.ToString("N0"));
            AppendMetricItem(sb, "Comments", projectMetrics.TotalCommentLines.ToString("N0"));
            AppendMetricItem(sb, "Comments Ratio", $"{projectMetrics.CommentsRatio:N1}%", GetCommentsRatioClass(projectMetrics.CommentsRatio));

            // Complexity metrics
            AppendMetricItem(sb, "Avg Complexity", $"{projectMetrics.AverageComplexity:N1}", GetComplexityClass(projectMetrics.AverageComplexity));
            AppendMetricItem(sb, "Maintainability", $"{projectMetrics.MaintainabilityIndex:N1}", GetMaintainabilityClass(projectMetrics.MaintainabilityIndex));

            // Duplication metrics
            if (projectMetrics.DuplicationRatio > 0)
            {
                AppendMetricItem(sb, "Duplicated Code", $"{projectMetrics.DuplicationRatio:N1}%", GetDuplicationClass(projectMetrics.DuplicationRatio));
            }

            // Issues count
            AppendMetricItem(sb, "Critical Issues", projectMetrics.CriticalIssues.ToString(), projectMetrics.CriticalIssues > 0 ? "poor" : "good");
            AppendMetricItem(sb, "High Issues", projectMetrics.HighIssues.ToString(), projectMetrics.HighIssues > 0 ? "average" : "good");
            AppendMetricItem(sb, "Medium Issues", projectMetrics.MediumIssues.ToString(), projectMetrics.MediumIssues > 5 ? "average" : "good");
            AppendMetricItem(sb, "Low Issues", projectMetrics.LowIssues.ToString());

            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");

            // Issues table
            if (projectMetrics.Issues.Count > 0)
            {
                sb.AppendLine("    <div class=\"metrics-card\">");
                sb.AppendLine("        <h2>Issues</h2>");
                sb.AppendLine("        <table>");
                sb.AppendLine("            <tr>");
                sb.AppendLine("                <th>Severity</th>");
                sb.AppendLine("                <th>Issue</th>");
                sb.AppendLine("                <th>Location</th>");
                sb.AppendLine("                <th>Suggestion</th>");
                sb.AppendLine("            </tr>");

                foreach (var issue in projectMetrics.Issues.OrderByDescending(i => i.Severity))
                {
                    sb.AppendLine("            <tr>");
                    sb.AppendLine($"                <td class=\"severity-{issue.Severity}\">{issue.Severity}</td>");
                    sb.AppendLine($"                <td>{issue.Title}</td>");
                    sb.AppendLine($"                <td>{Path.GetFileName(issue.FilePath)}: {issue.Line}</td>");
                    sb.AppendLine($"                <td>{issue.Suggestion}</td>");
                    sb.AppendLine("            </tr>");
                }

                sb.AppendLine("        </table>");
                sb.AppendLine("    </div>");
            }

            // Complex methods table
            var complexMethods = GetComplexMethods(projectMetrics);
            if (complexMethods.Count > 0)
            {
                sb.AppendLine("    <div class=\"metrics-card\">");
                sb.AppendLine("        <h2>Most Complex Methods</h2>");
                sb.AppendLine("        <table>");
                sb.AppendLine("            <tr>");
                sb.AppendLine("                <th>Complexity</th>");
                sb.AppendLine("                <th>Method</th>");
                sb.AppendLine("                <th>Class</th>");
                sb.AppendLine("                <th>File</th>");
                sb.AppendLine("                <th>Lines</th>");
                sb.AppendLine("            </tr>");

                foreach (var method in complexMethods)
                {
                    sb.AppendLine("            <tr>");
                    sb.AppendLine($"                <td class=\"{GetComplexityClass(method.complexity)}\">{method.complexity:N1}</td>");
                    sb.AppendLine($"                <td>{method.methodName}</td>");
                    sb.AppendLine($"                <td>{method.className}</td>");
                    sb.AppendLine($"                <td>{Path.GetFileName(method.filePath)}</td>");
                    sb.AppendLine($"                <td>{method.lineCount}</td>");
                    sb.AppendLine("            </tr>");
                }

                sb.AppendLine("        </table>");
                sb.AppendLine("    </div>");
            }

            // Long methods table
            var longMethods = GetLongMethods(projectMetrics);
            if (longMethods.Count > 0)
            {
                sb.AppendLine("    <div class=\"metrics-card\">");
                sb.AppendLine("        <h2>Longest Methods</h2>");
                sb.AppendLine("        <table>");
                sb.AppendLine("            <tr>");
                sb.AppendLine("                <th>Lines</th>");
                sb.AppendLine("                <th>Method</th>");
                sb.AppendLine("                <th>Class</th>");
                sb.AppendLine("                <th>File</th>");
                sb.AppendLine("                <th>Complexity</th>");
                sb.AppendLine("            </tr>");

                foreach (var method in longMethods)
                {
                    sb.AppendLine("            <tr>");
                    sb.AppendLine($"                <td class=\"{GetMethodLengthClass(method.lineCount)}\">{method.lineCount}</td>");
                    sb.AppendLine($"                <td>{method.methodName}</td>");
                    sb.AppendLine($"                <td>{method.className}</td>");
                    sb.AppendLine($"                <td>{Path.GetFileName(method.filePath)}</td>");
                    sb.AppendLine($"                <td>{method.complexity:N1}</td>");
                    sb.AppendLine("            </tr>");
                }

                sb.AppendLine("        </table>");
                sb.AppendLine("    </div>");
            }

            // File metrics table
            sb.AppendLine("    <div class=\"metrics-card\">");
            sb.AppendLine("        <h2>File Metrics</h2>");
            sb.AppendLine("        <table>");
            sb.AppendLine("            <tr>");
            sb.AppendLine("                <th>File</th>");
            sb.AppendLine("                <th>Lines</th>");
            sb.AppendLine("                <th>Complexity</th>");
            sb.AppendLine("                <th>Maintainability</th>");
            sb.AppendLine("                <th>Issues</th>");
            sb.AppendLine("            </tr>");

            foreach (var file in projectMetrics.Files.OrderByDescending(f => f.Issues.Count))
            {
                sb.AppendLine("            <tr>");
                sb.AppendLine($"                <td>{Path.GetFileName(file.FilePath)}</td>");
                sb.AppendLine($"                <td>{file.CodeLines:N0}</td>");
                sb.AppendLine($"                <td class=\"{GetComplexityClass(file.CyclomaticComplexity)}\">{file.CyclomaticComplexity:N1}</td>");
                sb.AppendLine($"                <td class=\"{GetMaintainabilityClass(file.MaintainabilityIndex)}\">{file.MaintainabilityIndex:N1}</td>");
                sb.AppendLine($"                <td>{file.Issues.Count}</td>");
                sb.AppendLine("            </tr>");
            }

            sb.AppendLine("        </table>");
            sb.AppendLine("    </div>");

            // End HTML document
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private void AppendMetricItem(StringBuilder sb, string name, string value, string cssClass = "")
        {
            sb.AppendLine("            <div class=\"metric-item\">");
            sb.AppendLine($"                <div class=\"metric-value {cssClass}\">{value}</div>");
            sb.AppendLine($"                <div class=\"metric-name\">{name}</div>");
            sb.AppendLine("            </div>");
        }

        private string GetComplexityClass(double complexity)
        {
            if (complexity >= 25) return "poor";
            if (complexity >= 15) return "average";
            return "good";
        }

        private string GetMaintainabilityClass(double index)
        {
            if (index < 50) return "poor";
            if (index < 70) return "average";
            return "good";
        }

        private string GetCommentsRatioClass(double ratio)
        {
            if (ratio < 5) return "poor";
            if (ratio < 15) return "average";
            if (ratio > 50) return "average"; // Too many comments can also be a problem
            return "good";
        }

        private string GetDuplicationClass(double ratio)
        {
            if (ratio > 20) return "poor";
            if (ratio > 10) return "average";
            return "good";
        }

        private string GetMethodLengthClass(int lines)
        {
            if (lines > 100) return "poor";
            if (lines > 50) return "average";
            if (lines > 30) return "average";
            return "good";
        }

        private List<(double complexity, string methodName, string className, string filePath, int lineCount)>
            GetComplexMethods(ProjectMetrics projectMetrics)
        {
            var methods = new List<(double complexity, string methodName, string className, string filePath, int lineCount)>();

            foreach (var file in projectMetrics.Files)
            {
                foreach (var classMetric in file.Classes)
                {
                    foreach (var methodMetric in classMetric.Methods)
                    {
                        methods.Add((
                            methodMetric.CyclomaticComplexity,
                            methodMetric.MethodName,
                            classMetric.ClassName,
                            file.FilePath,
                            methodMetric.LineCount
                        ));
                    }
                }
            }

            return methods.OrderByDescending(m => m.complexity).Take(10).ToList();
        }

        private List<(int lineCount, string methodName, string className, string filePath, double complexity)>
            GetLongMethods(ProjectMetrics projectMetrics)
        {
            var methods = new List<(int lineCount, string methodName, string className, string filePath, double complexity)>();

            foreach (var file in projectMetrics.Files)
            {
                foreach (var classMetric in file.Classes)
                {
                    foreach (var methodMetric in classMetric.Methods)
                    {
                        methods.Add((
                            methodMetric.LineCount,
                            methodMetric.MethodName,
                            classMetric.ClassName,
                            file.FilePath,
                            methodMetric.CyclomaticComplexity
                        ));
                    }
                }
            }

            return methods.OrderByDescending(m => m.lineCount).Take(10).ToList();
        }
    }
}