using System.Linq;
using BlazorMonaco.Editor;
using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.Services;
using devbuddy.plugins.MyNotes.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace devbuddy.plugins.MyNotes
{
    public partial class NotesIndex : AppComponentBase<MyNotesDataModel>
    {
        private ModalComponentBase _deleteModal;
        private StandaloneCodeEditor _codeEditor;
        private string NewTag { get; set; } = string.Empty;
        private bool _contentChanged = false, _titleChanged = false;
        private Note? CurrentNote => Model.Notes.FirstOrDefault(n => n.Id == Model.LastOpenedNoteId);

        protected override async Task OnInitializedAsync()
        {
            // Carica i dati dal servizio
            Model = DataModelService.ValueByKey<MyNotesDataModel>(nameof(MyNotes));
            await base.OnInitializedAsync();
        }

        private async Task SelectNote(Note note)
        {
            Model.LastOpenedNoteId = note.Id;
            _codeEditor?.SetValue(CurrentNote?.Content);
            await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
            StateHasChanged();
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = CurrentNote.Language,
            };
        }

        private async Task AddNote(Note note)
        {
            Model.Notes.Add(note);
            Model.LastOpenedNoteId = note.Id;
            await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
            // Forza l'aggiornamento dell'interfaccia
            StateHasChanged();
            ToastService.Show("Nuova nota creata", ToastLevel.Success);
        }

        private async Task NoteChanged(Note note)
        {
            // Trova l'indice della nota esistente
            var index = Model.Notes.FindIndex(n => n.Id == note.Id);

            // Se la nota esiste, aggiornala
            if (index >= 0)
            {
                Model.Notes[index] = note;
                await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
            }
        }


        private async Task OnContentChanged(ModelContentChangedEvent e)
        {
            if (CurrentNote != null)
            {
                CurrentNote.Content = await _codeEditor.GetValue();
                CurrentNote.ModifiedAt = DateTime.Now;
                _contentChanged = true;
            }
        }

        private async Task OnTitleChanged()
        {
            if (CurrentNote != null)
            {
                CurrentNote.ModifiedAt = DateTime.Now;
                _titleChanged = true;
            }
        }

        private async Task ToggleFavorite()
        {
            if (CurrentNote != null)
            {
                CurrentNote.IsFavorite = !CurrentNote.IsFavorite;
                CurrentNote.ModifiedAt = DateTime.Now;

                var message = CurrentNote.IsFavorite ?
                    "Nota aggiunta ai preferiti" :
                    "Nota rimossa dai preferiti";

                ToastService.Show(message, ToastLevel.Success);
            }
        }

        private void DeleteNote()
        {
            if (CurrentNote != null)
            {
                _deleteModal.Show();
            }
        }

        private async Task ConfirmDelete()
        {
            if (CurrentNote != null)
            {
                Model.Notes.RemoveAll(n => n.Id == CurrentNote.Id);

                // Se la nota cancellata era quella attualmente selezionata, deseleziona
                if (Model.LastOpenedNoteId == CurrentNote.Id)
                {
                    Model.LastOpenedNoteId = Model.Notes.FirstOrDefault()?.Id;
                }

                await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
                ToastService.Show("Nota eliminata.", ToastLevel.Success);
                StateHasChanged();
            }
        }

        private async Task AddTag()
        {
            if (CurrentNote != null && !string.IsNullOrWhiteSpace(NewTag))
            {
                // Normalize tag
                var tag = NewTag.Trim().ToLowerInvariant();

                // Check if tag already exists
                if (!CurrentNote.Tags.Contains(tag))
                {
                    CurrentNote.Tags.Add(tag);
                    CurrentNote.ModifiedAt = DateTime.Now;
                }

                NewTag = string.Empty;
            }
        }

        private async Task RemoveTag(string tag)
        {
            if (CurrentNote != null)
            {
                CurrentNote.Tags.Remove(tag);
                CurrentNote.ModifiedAt = DateTime.Now;
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