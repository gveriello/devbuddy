using System.Text;
using devbuddy.common.Applications;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.Base64
{
    public partial class Index : AppComponentBase
    {
        private string _inputText = string.Empty;
        private string _outputText = string.Empty;
        private string _conversionMode = "encode";
        private bool _autoConvert = true;
        private string _errorMessage = string.Empty;

        [Inject] private IJSRuntime JSRuntime { get; set; }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                if (_autoConvert)
                {
                    ConvertText();
                }
            }
        }

        public string OutputText
        {
            get => _outputText;
            set => _outputText = value;
        }

        public string ConversionMode
        {
            get => _conversionMode;
            set => _conversionMode = value;
        }

        public bool AutoConvert
        {
            get => _autoConvert;
            set => _autoConvert = value;
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => _errorMessage = value;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        public void SetConversionMode(string mode)
        {
            ConversionMode = mode;
            if (AutoConvert && !string.IsNullOrEmpty(InputText))
            {
                ConvertText();
            }
        }

        public void ToggleAutoConvert(object value)
        {
            if (value is bool boolValue)
            {
                AutoConvert = boolValue;
                if (AutoConvert && !string.IsNullOrEmpty(InputText))
                {
                    ConvertText();
                }
            }
        }

        public void ConvertText()
        {
            ErrorMessage = string.Empty;

            try
            {
                if (ConversionMode == "encode")
                {
                    // Testo a Base64
                    if (string.IsNullOrEmpty(InputText))
                    {
                        OutputText = string.Empty;
                        return;
                    }

                    byte[] textBytes = Encoding.UTF8.GetBytes(InputText);
                    OutputText = Convert.ToBase64String(textBytes);
                }
                else
                {
                    // Base64 a testo
                    if (string.IsNullOrEmpty(InputText))
                    {
                        OutputText = string.Empty;
                        return;
                    }

                    // Verifica se l'input è in formato Base64 valido
                    try
                    {
                        byte[] textBytes = Convert.FromBase64String(InputText);
                        OutputText = Encoding.UTF8.GetString(textBytes);
                    }
                    catch (FormatException)
                    {
                        ErrorMessage = "L'input non è in formato Base64 valido.";
                        OutputText = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore durante la conversione: {ex.Message}";
                OutputText = string.Empty;
            }
        }

        public async Task PasteFromClipboard()
        {
            try
            {
                var clipboardText = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
                InputText = clipboardText;

                // La conversione avverrà automaticamente se AutoConvert è abilitato
                // grazie al setter di InputText
                if (!AutoConvert)
                {
                    ConvertText();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore nell'accesso alla clipboard: {ex.Message}";
            }
        }

        public void ClearInput()
        {
            InputText = string.Empty;
            OutputText = string.Empty;
            ErrorMessage = string.Empty;
        }

        public async Task CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(OutputText))
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", OutputText);
                ToastService.Show("Copiato negli appunti");
            }
        }
    }
}
