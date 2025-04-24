using System.Reflection;
using devbuddy.common.Applications;
using devbuddy.common.Services;
using devbuddy.plugins.LoremIpsum.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.LoremIpsum
{
    public partial class Index : AppComponentBase
    {
        private string generatedText = string.Empty;

        // Generator parameters
        private int paragraphs = 3;
        private int sentencesPerParagraph = 5;
        private int wordsPerSentence = 8;
        private bool startWithLoremIpsum = true;
        private bool includeLatinText = true;
        private LoremIpsumService _loremIpsumService;

        protected override async Task OnInitializedAsync()
        {
            this._loremIpsumService = new();

            // Generate initial text
            GenerateText();
        }

        private void GenerateText()
        {
            generatedText = _loremIpsumService.Generate(
                paragraphs,
                sentencesPerParagraph,
                wordsPerSentence,
                startWithLoremIpsum,
                includeLatinText);
        }

        private async Task CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(generatedText))
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", generatedText);
                ToastService.Show("Testo copiato negli appunti", ToastLevel.Success);
            }
        }

        private void ResetToDefaults()
        {
            // Reset to system defaults
            paragraphs = 3;
            sentencesPerParagraph = 5;
            wordsPerSentence = 8;
            startWithLoremIpsum = true;
            includeLatinText = true;

            ToastService.Show("Impostazioni ripristinate ai valori predefiniti", ToastLevel.Success);
        }
    }
}