using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.JsonFormatters
{

    public sealed partial class Index
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        private string InputJson { get; set; } = string.Empty;
        private string OutputJson { get; set; } = string.Empty;
        private string ErrorMessage { get; set; } = string.Empty;

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
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"JSON non valido: {ex.Message}";
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
            }
        }
    }
}
