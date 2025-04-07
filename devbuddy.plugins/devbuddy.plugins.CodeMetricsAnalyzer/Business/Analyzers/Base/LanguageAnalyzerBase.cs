using devbuddy.plugins.CodeMetricsAnalyzer.Models;

namespace devbuddy.plugins.CodeMetricsAnalyzer.Business.Analyzers.Base
{
    public abstract class LanguageAnalyzerBase
    {
        public abstract Task AnalyzeFileAsync(string fileContent, FileMetrics fileMetrics);

        protected int CountCodeLines(string[] lines)
        {
            int codeLines = 0;
            bool inMultilineComment = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Handle language-specific comment patterns
                if (IsCommentLine(trimmedLine, ref inMultilineComment))
                    continue;

                codeLines++;
            }

            return codeLines;
        }

        protected int CountCommentLines(string[] lines)
        {
            int commentLines = 0;
            bool inMultilineComment = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Handle language-specific comment patterns
                if (IsCommentLine(trimmedLine, ref inMultilineComment))
                    commentLines++;
            }

            return commentLines;
        }

        protected abstract bool IsCommentLine(string line, ref bool inMultilineComment);

        protected double CalculateMaintainabilityIndex(int linesOfCode, double cyclomaticComplexity, int distinctOperators)
        {
            // Implementazione semplificata dell'indice di manutenibilità
            // Normalmente si usa una formula più complessa che include anche il volume di Halstead
            double maintainabilityIndex = Math.Max(0, 171 - 5.2 * Math.Log(linesOfCode) - 0.23 * cyclomaticComplexity - 16.2 * Math.Log(distinctOperators));

            // Normalizza su una scala da 0 a 100
            return Math.Min(100, Math.Max(0, maintainabilityIndex * 100 / 171));
        }
    }
}