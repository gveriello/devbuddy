using System.Reflection;
using devbuddy.common.Applications;
using devbuddy.common.Services;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.Markdown
{
    public partial class Index : AppComponentBase
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private ToastService ToastService { get; set; }

        private string _inputMarkdown = string.Empty;
        private string _renderedMarkdown = string.Empty;
        private string _renderedHtml = string.Empty;
        private string _errorMessage = string.Empty;
        private string activeTab = "editor";

        // Modal references and properties
        private ModalComponentBase saveModal;
        private ModalComponentBase deleteModal;
        private string SaveMarkdownName { get; set; } = string.Empty;
        private string SaveMarkdownDescription { get; set; } = string.Empty;

        // Markdig pipeline for advanced features
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public string InputMarkdown
        {
            get => _inputMarkdown;
            set
            {
                _inputMarkdown = value;
                RenderMarkdown();
            }
        }

        public string RenderedMarkdown
        {
            get => _renderedMarkdown;
            set => _renderedMarkdown = value;
        }

        public string RenderedHtml
        {
            get => _renderedHtml;
            set => _renderedHtml = value;
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => _errorMessage = value;
        }

        public void RenderMarkdown()
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(InputMarkdown))
            {
                RenderedMarkdown = string.Empty;
                RenderedHtml = string.Empty;
                return;
            }

            try
            {
                // Render markdown to HTML using Markdig
                RenderedHtml = Markdig.Markdown.ToHtml(InputMarkdown, _pipeline);
                RenderedMarkdown = RenderedHtml;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore durante la conversione del Markdown: {ex.Message}";
                RenderedMarkdown = string.Empty;
                RenderedHtml = string.Empty;
            }
        }

        public async Task PasteFromClipboard()
        {
            try
            {
                var clipboardText = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
                InputMarkdown = clipboardText;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore nell'accesso alla clipboard: {ex.Message}";
            }
        }

        public void ClearInput()
        {
            InputMarkdown = string.Empty;
            RenderedMarkdown = string.Empty;
            RenderedHtml = string.Empty;
            ErrorMessage = string.Empty;
        }

        public async Task CopyHtmlToClipboard()
        {
            if (!string.IsNullOrEmpty(RenderedHtml))
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", RenderedHtml);
                ToastService.Show("HTML copiato negli appunti", ToastLevel.Success);
            }
        }

        public void ShowSaveDialog()
        {
            if (string.IsNullOrWhiteSpace(InputMarkdown))
            {
                ToastService.Show("Non c'è nulla da salvare. Inserisci prima un Markdown valido.", ToastLevel.Warning);
                return;
            }

            SaveMarkdownName = string.Empty;
            SaveMarkdownDescription = string.Empty;
            saveModal.Show();
        }
    }
}