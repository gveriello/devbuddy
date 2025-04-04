using devbuddy.plugins.QueryOptimizer.Business;
using devbuddy.plugins.QueryOptimizer.Business.Models.Enum;
using devbuddy.plugins.QueryOptimizer.Business.Plan;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.QueryOptimizer
{
    public sealed partial class Index
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        private string OriginalQuery { get; set; } = @"";
        private string OptimizedQuery { get; set; } = "";
        private string SchemaJson { get; set; } = @"";
        private string SelectedDbType { get; set; } = "sqlserver";
        private bool IsAnalyzing { get; set; } = false;
        private OptimizationResult Result { get; set; }
        private string ActiveTab { get; set; } = "issues";
        private bool ShowAdvancedFeatures { get; set; } = false;
        private BenchmarkResult BenchmarkResults { get; set; }
        private string ShareUrl { get; set; } = "";
        private bool LinkCopied { get; set; } = false;

        private Optimizer _optimizer;

        protected override void OnInitialized()
        {
            _optimizer = new Optimizer(JSRuntime, SelectedDbType);
        }

        private async Task AnalyzeQuery()
        {
            if (string.IsNullOrWhiteSpace(OriginalQuery))
            {
                return;
            }

            IsAnalyzing = true;
            StateHasChanged();

            // Small delay to show the loading indicator
            await Task.Delay(500);

            Result = await _optimizer.OptimizeAsync(OriginalQuery, SchemaJson);

            if (!string.IsNullOrWhiteSpace(Result.OptimizedQuery))
            {
                OptimizedQuery = Result.OptimizedQuery;
            }

            IsAnalyzing = false;
            StateHasChanged();
        }

        private void ResetResults()
        {
            Result = null;
            OptimizedQuery = "";
            ActiveTab = "issues";
            BenchmarkResults = null;
        }

        private void ApplySuggestion(OptimizationSuggestion suggestion)
        {
            if (!string.IsNullOrWhiteSpace(suggestion.OptimizedQuery))
            {
                OptimizedQuery = suggestion.OptimizedQuery;
            }
        }

        private string GetSeverityClass(SeverityLevel severity)
        {
            return severity switch
            {
                SeverityLevel.Critical => "danger",
                SeverityLevel.High => "warning",
                SeverityLevel.Medium => "info",
                SeverityLevel.Low => "secondary",
                _ => "secondary"
            };
        }

        private RenderFragment RenderExecutionPlanNode(ExecutionPlanNode node, int level) => builder =>
        {
            if (node == null) return;

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "plan-node");
            builder.AddAttribute(2, "style", $"margin-left: {level * 20}px");

            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", "d-flex");

            builder.OpenElement(5, "span");
            builder.AddAttribute(6, "class", "node-icon mr-2");
            builder.AddContent(7, level == 0 ? "➤" : "└─");
            builder.CloseElement(); // span

            builder.OpenElement(8, "div");
            builder.AddAttribute(9, "class", "node-details");

            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", "node-operation");
            builder.AddContent(12, node.Operation);
            if (!string.IsNullOrEmpty(node.ObjectName))
            {
                builder.AddContent(13, $" on {node.ObjectName}");
            }
            builder.CloseElement(); // div

            builder.OpenElement(14, "small");
            builder.AddAttribute(15, "class", "text-muted");
            builder.AddContent(16, $"Cost: {node.Cost}, Rows: {node.Rows}");
            builder.CloseElement(); // small

            builder.CloseElement(); // div

            builder.CloseElement(); // div

            // Render children
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    builder.AddContent(17, RenderExecutionPlanNode(child, level + 1));
                }
            }

            builder.CloseElement(); // div
        };

        private void ToggleAdvancedFeatures()
        {
            ShowAdvancedFeatures = !ShowAdvancedFeatures;
        }

        private async Task AnalyzeIndices()
        {
            // In una implementazione reale, qui chiameremmo un servizio per analizzare gli indici
            await Task.Delay(1000); // Simuliamo un'operazione asincrona

            // Qui implementeremmo l'analisi degli indici e visualizzeremmo i risultati
            await JSRuntime.InvokeVoidAsync("alert", "Analisi indici completata. Suggerimento: Aggiungere un indice sulla colonna orders.created_at per migliorare le performance delle ricerche per data.");
        }

        private async Task RunBenchmark()
        {
            // In una implementazione reale, qui eseguiremmo un vero benchmark
            IsAnalyzing = true;
            StateHasChanged();

            // Simuliamo un'operazione che richiede tempo
            await Task.Delay(2000);

            // Simuliamo i risultati del benchmark
            BenchmarkResults = new BenchmarkResult
            {
                OriginalExecutionTime = 450,
                OptimizedExecutionTime = 120,
                OriginalCpuUsage = 65,
                OptimizedCpuUsage = 30,
                OriginalDiskIO = 8500,
                OptimizedDiskIO = 2200,
                OriginalMemoryUsage = 245,
                OptimizedMemoryUsage = 180
            };

            IsAnalyzing = false;
            StateHasChanged();
        }

        private async Task ExportPdf()
        {
            // In una implementazione reale, qui genereremmo un PDF
            await JSRuntime.InvokeVoidAsync("alert", "La funzione di esportazione PDF sarà disponibile nella prossima versione.");
        }

        private async Task ShareLink()
        {
            // In una implementazione reale, qui genereremmo un link condivisibile
            // che potrebbe essere caricato da altri utenti per vedere gli stessi risultati

            // Genera un ID univoco per questa analisi
            string analysisId = Guid.NewGuid().ToString("N");

            // Crea un URL che include tutti i parametri necessari
            ShareUrl = $"https://tuodominio.it/query-optimizer/share/{analysisId}";

            // Salva i risultati dell'analisi (in una implementazione reale questo sarebbe in un database)
            // await QueryAnalysisService.SaveAnalysis(analysisId, Result, OriginalQuery, OptimizedQuery);

            // Mostra il modal
            await JSRuntime.InvokeVoidAsync("$('#shareModal').modal", "show");

            LinkCopied = false;
            StateHasChanged();
        }

        private async Task CopyShareLink()
        {
            // Copia il link negli appunti
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", ShareUrl);

            LinkCopied = true;
            StateHasChanged();

            // Resetta lo stato dopo 3 secondi
            await Task.Delay(3000);
            LinkCopied = false;
            StateHasChanged();
        }

        // Classe per i risultati del benchmark
        private class BenchmarkResult
        {
            public int OriginalExecutionTime { get; set; } // in ms
            public int OptimizedExecutionTime { get; set; } // in ms
            public int OriginalCpuUsage { get; set; } // in %
            public int OptimizedCpuUsage { get; set; } // in %
            public int OriginalDiskIO { get; set; } // in KB
            public int OptimizedDiskIO { get; set; } // in KB
            public int OriginalMemoryUsage { get; set; } // in MB
            public int OptimizedMemoryUsage { get; set; } // in MB
        }
    }
}
