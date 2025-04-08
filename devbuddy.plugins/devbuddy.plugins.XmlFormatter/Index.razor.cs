using System.Xml;
using System.Xml.Linq;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.XmlFormatter
{
    public sealed partial class Index
    {
        [Inject] private IJSRuntime? JSRuntime { get; set; }
        [Inject] private ToastService? ToastService { get; set; }

        private string InputXml { get; set; } = string.Empty;
        private string OutputXml { get; set; } = string.Empty;
        private string ErrorMessage { get; set; } = string.Empty;
        private int XmlSize { get; set; } = 0;
        private int XmlDepth { get; set; } = 0;
        private int XmlElements { get; set; } = 0;

        private bool _autoFormat = true;
        private bool _removeWhitespace = false;
        private bool _omitDeclaration = false;

        public bool AutoFormat
        {
            get => _autoFormat;
            set => _autoFormat = value;
        }

        public bool RemoveWhitespace
        {
            get => _removeWhitespace;
            set
            {
                _removeWhitespace = value;
                if (_autoFormat) FormatXml();
            }
        }

        public bool OmitDeclaration
        {
            get => _omitDeclaration;
            set
            {
                _omitDeclaration = value;
                if (_autoFormat) FormatXml();
            }
        }

        public void FormatXml()
        {
            if (string.IsNullOrWhiteSpace(InputXml))
            {
                OutputXml = string.Empty;
                ErrorMessage = string.Empty;
                XmlSize = 0;
                XmlDepth = 0;
                XmlElements = 0;
                return;
            }

            try
            {
                // Rimuove gli spazi bianchi se richiesto
                string xmlToFormat = InputXml;
                if (RemoveWhitespace)
                {
                    xmlToFormat = RemoveXmlWhitespace(xmlToFormat);
                }

                // Carica l'XML in un XDocument
                XDocument doc = XDocument.Parse(xmlToFormat);

                // Calcola le statistiche dell'XML
                XmlSize = OutputXml.Length;
                CalculateXmlMetrics(doc);

                // Formatta l'XML
                var xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace,
                    OmitXmlDeclaration = OmitDeclaration
                };

                using var stringWriter = new System.IO.StringWriter();
                using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
                {
                    doc.Save(xmlWriter);
                }

                OutputXml = stringWriter.ToString();
                XmlSize = OutputXml.Length;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"XML non valido: {ex.Message}";
                OutputXml = string.Empty;
                XmlSize = 0;
                XmlDepth = 0;
                XmlElements = 0;
            }
        }

        private string RemoveXmlWhitespace(string xml)
        {
            try
            {
                // Carica l'XML rimuovendo spazi bianchi non significativi
                XDocument doc = XDocument.Parse(xml, LoadOptions.None);

                // Rimuove spazi bianchi tra gli elementi
                foreach (var node in doc.DescendantNodes().OfType<XText>().Where(t => string.IsNullOrWhiteSpace(t.Value)).ToList())
                {
                    node.Remove();
                }

                // Ritorna l'XML senza gli spazi bianchi
                return doc.ToString(SaveOptions.DisableFormatting);
            }
            catch
            {
                // Se fallisce, ritorna l'XML originale
                return xml;
            }
        }

        private void CalculateXmlMetrics(XDocument doc)
        {
            try
            {
                // Conta il numero di elementi
                XmlElements = doc.Descendants().Count();

                // Calcola la profondità massima
                XmlDepth = CalculateMaxDepth(doc.Root);
            }
            catch
            {
                XmlElements = 0;
                XmlDepth = 0;
            }
        }

        private int CalculateMaxDepth(XElement element, int currentDepth = 1)
        {
            if (element == null) return 0;

            int maxChildDepth = 0;
            foreach (var child in element.Elements())
            {
                int childDepth = CalculateMaxDepth(child, currentDepth + 1);
                maxChildDepth = Math.Max(maxChildDepth, childDepth);
            }

            return Math.Max(currentDepth, maxChildDepth);
        }

        public void ToggleAutoFormat(object value)
        {
            if (value is bool boolValue)
            {
                AutoFormat = boolValue;
                if (AutoFormat && !string.IsNullOrEmpty(InputXml))
                {
                    FormatXml();
                }
            }
        }

        public void ToggleRemoveWhitespace(object value)
        {
            if (value is bool boolValue)
            {
                RemoveWhitespace = boolValue;
            }
        }

        public void ToggleOmitDeclaration(object value)
        {
            if (value is bool boolValue)
            {
                OmitDeclaration = boolValue;
            }
        }

        public async Task PasteFromClipboard()
        {
            try
            {
                var clipboardText = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
                InputXml = clipboardText;

                if (AutoFormat)
                {
                    FormatXml();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore nell'accesso alla clipboard: {ex.Message}";
            }
        }

        public void ClearInput()
        {
            InputXml = string.Empty;
            OutputXml = string.Empty;
            ErrorMessage = string.Empty;
            XmlSize = 0;
            XmlDepth = 0;
            XmlElements = 0;
        }

        public async Task CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(OutputXml))
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", OutputXml);
                ToastService.Show("Copiato negli appunti");
            }
        }
    }
}