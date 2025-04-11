using BlazorMonaco.Editor;
using devbuddy.common.Applications;
using devbuddy.common.Services;
using devbuddy.plugins.MyNotes.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace devbuddy.plugins.MyNotes
{
    public partial class NoteEditor : AppComponentBase
    {
        [Parameter]
        public Note? Note { get; set; }

        [Parameter]
        public EventCallback<Note> OnNoteChanged { get; set; }

        [Parameter]
        public EventCallback<Note> OnNoteDeleted { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        private ModalComponentBase _deleteModal;
        private StandaloneCodeEditor _codeEditor;

        private string NewTag { get; set; } = string.Empty;
        private bool _contentChanged = false;
        private bool _titleChanged = false;

        protected override Task OnInitializedAsync()
        {
            _codeEditor?.SetValue(Note.Content);
            return base.OnInitializedAsync();
        }
        private StandaloneEditorConstructionOptions _options => new()
        {
            AutomaticLayout = true,
            Language = Note?.Language,
            Value = Note?.Content
        };

        private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = Note?.Language,
                Value = Note?.Content
            };
        }

        private async Task OnContentChanged(ModelContentChangedEvent e)
        {
            if (Note != null)
            {
                Note.Content = await _codeEditor.GetValue();
                Note.ModifiedAt = DateTime.Now;
                _contentChanged = true;
                await OnNoteChanged.InvokeAsync(Note);
            }
        }

        private async Task OnTitleChanged()
        {
            if (Note != null)
            {
                Note.ModifiedAt = DateTime.Now;
                _titleChanged = true;
                await OnNoteChanged.InvokeAsync(Note);
            }
        }

        private async Task OnTypeChanged()
        {
            if (Note != null)
            {
                // Reset language if changed from code to another type
                if (Note.Type != NoteType.Code)
                {
                    Note.Language = "plaintext";
                }

                Note.ModifiedAt = DateTime.Now;
                await OnNoteChanged.InvokeAsync(Note);
            }
        }

        private async Task ToggleFavorite()
        {
            if (Note != null)
            {
                Note.IsFavorite = !Note.IsFavorite;
                Note.ModifiedAt = DateTime.Now;
                await OnNoteChanged.InvokeAsync(Note);

                var message = Note.IsFavorite ?
                    "Nota aggiunta ai preferiti" :
                    "Nota rimossa dai preferiti";

                ToastService.Show(message, ToastLevel.Success);
            }
        }

        private void DeleteNote()
        {
            if (Note != null)
            {
                _deleteModal.Show();
            }
        }

        private async Task ConfirmDelete()
        {
            if (Note != null)
            {
                await OnNoteDeleted.InvokeAsync(Note);
                ToastService.Show("Nota eliminata", ToastLevel.Success);
            }
        }

        private async Task AddTag()
        {
            if (Note != null && !string.IsNullOrWhiteSpace(NewTag))
            {
                // Normalize tag
                var tag = NewTag.Trim().ToLowerInvariant();

                // Check if tag already exists
                if (!Note.Tags.Contains(tag))
                {
                    Note.Tags.Add(tag);
                    Note.ModifiedAt = DateTime.Now;
                    await OnNoteChanged.InvokeAsync(Note);
                }

                NewTag = string.Empty;
            }
        }

        private async Task RemoveTag(string tag)
        {
            if (Note != null)
            {
                Note.Tags.Remove(tag);
                Note.ModifiedAt = DateTime.Now;
                await OnNoteChanged.InvokeAsync(Note);
            }
        }

        private async Task HandleTagInputKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(NewTag))
            {
                await AddTag();
            }
        }
    }
}