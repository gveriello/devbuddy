using System.Text.Json;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.JsonFormatters
{

    public sealed partial class Index
    {
        [Inject] private IJSRuntime? JSRuntime { get; set; }
        [Inject] private ToastService? ToastService { get; set; }
        private string InputJson { get; set; } = string.Empty;
        private string OutputJson { get; set; } = string.Empty;
        private string ErrorMessage { get; set; } = string.Empty;
        private int JsonSize { get; set; } = 0;
        private int JsonDepth { get; set; } = 0;
        private int JsonProperties { get; set; } = 0;

        private void FormatJson()
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                OutputJson = string.Empty;
                ErrorMessage = string.Empty;
                return;
            }

            try
            {
                // Prova a deserializzare e successivamente serializzare con indentazione
                var jsonObject = System.Text.Json.JsonSerializer.Deserialize<object>(InputJson);
                OutputJson = System.Text.Json.JsonSerializer.Serialize(
                    jsonObject,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                // Calcola le statistiche del JSON
                JsonSize = OutputJson.Length;
                CalculateJsonMetrics(jsonObject);
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"JSON non valido: {ex.Message}";
            }

            void CalculateJsonMetrics(object jsonObject)
            {
                JsonProperties = 0;
                JsonDepth = 0;

                if (jsonObject != null)
                {
                    // Calcola profondità e numero di proprietà
                    CalculateDepthAndProperties(jsonObject, 1);
                }

                void CalculateDepthAndProperties(object obj, int currentDepth)
                {
                    // Aggiorna la profondità massima se necessario
                    if (currentDepth > JsonDepth)
                    {
                        JsonDepth = currentDepth;
                    }

                    if (obj is JsonElement element)
                    {
                        switch (element.ValueKind)
                        {
                            case JsonValueKind.Object:
                                foreach (var property in element.EnumerateObject())
                                {
                                    JsonProperties++;
                                    CalculateDepthAndProperties(property.Value, currentDepth + 1);
                                }
                                break;
                            case JsonValueKind.Array:
                                foreach (var item in element.EnumerateArray())
                                {
                                    CalculateDepthAndProperties(item, currentDepth + 1);
                                }
                                break;
                        }
                    }
                }
            }


        }

        private async Task PasteFromClipboard()
        {
            try
            {
                var clipboardText = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
                InputJson = clipboardText;
                FormatJson();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore nell'accesso alla clipboard: {ex.Message}";
            }
        }

        private void ClearInput()
        {
            InputJson = string.Empty;
            OutputJson = string.Empty;
            ErrorMessage = string.Empty;
        }

        private async Task CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(OutputJson))
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", OutputJson);
                ToastService.Show("Copiato");
            }
        }
    }
}
