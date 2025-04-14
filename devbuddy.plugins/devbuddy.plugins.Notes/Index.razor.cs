using BlazorMonaco.Editor;
using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.ExtensionMethods;
using devbuddy.common.Services;
using devbuddy.plugins.Notes.Models;
using Microsoft.AspNetCore.Components.Web;

namespace devbuddy.plugins.Notes
{
    public partial class Index : AppComponentBase<NotesDataModel>
    {
        private ModalComponentBase _deleteModal;
        private StandaloneCodeEditor _codeEditor;
        private readonly string _apiKey = ModulesItems.Notes.AttributeValueOrDefault<ModuleKeyAttribute, string>(attr => attr.Key);
        private string NewTag { get; set; } = string.Empty;
        private bool _contentChanged = false, _titleChanged = false;

        private Note? SelectedNode { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // Carica i dati dal servizio
            Model = await DataModelService.GetDataModelByApiKey<NotesDataModel>(_apiKey);
            await base.OnInitializedAsync();
        }

        private async Task OnNoteSelectionChanged(Note note)
        {
            Model.LastOpenedNoteId = note.Id;
            SelectedNode = Model.Notes.FirstOrDefault(n => n.Id == Model.LastOpenedNoteId);
            _codeEditor?.SetValue(SelectedNode?.Content);
            await DataModelService.SaveChangesAsync(_apiKey, Model);
            StateHasChanged();
        }

        private void OnLanguageChanged()
        {

        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                Language = SelectedNode.Language,
            };
        }

        private async Task NoteChanged(Note note)
        {
            // Trova l'indice della nota esistente
            var index = Model.Notes.FindIndex(n => n.Id == note.Id);

            // Se la nota esiste, aggiornala
            if (index >= 0)
            {
                Model.Notes[index] = note;
                await DataModelService.SaveChangesAsync(_apiKey, Model);
            }
        }


        private async Task OnContentChanged(ModelContentChangedEvent e)
        {
            if (SelectedNode != null)
            {
                SelectedNode.Content = await _codeEditor.GetValue();
                _codeEditor.ConstructionOptions.Invoke(_codeEditor);
                SelectedNode.ModifiedAt = DateTime.Now;
                _contentChanged = true;
            }
        }

        private async Task OnTitleChanged()
        {
            if (SelectedNode != null)
            {
                SelectedNode.ModifiedAt = DateTime.Now;
                _titleChanged = true;
            }
        }

        private string SearchQuery { get; set; } = string.Empty;
        private NoteFilterType FilterType { get; set; } = NoteFilterType.All;
        private List<string> SelectedTags { get; set; } = [];

        private List<Note> FilteredNotes => [.. Model.Notes
            .Where(note => FilterBySearch(note))
            .Where(note => FilterByType(note))
            .Where(note => FilterByTags(note))
            .OrderByDescending(note => note.ModifiedAt)];

        private List<string> AllTags => [.. Model.Notes
            .SelectMany(note => note.Tags)
            .Distinct()
            .OrderBy(tag => tag)];

        private static string GetNoteTypeIconClass(NoteType type)
        {
            return type switch
            {
                NoteType.Code => "fa-solid fa-code",
                NoteType.Markdown => "fa-brands fa-markdown",
                _ => "fa-solid fa-file-lines"
            };
        }

        private static string GetNoteTypeTitle(NoteType type)
        {
            return type switch
            {
                NoteType.Code => "Codice",
                NoteType.Markdown => "Markdown",
                _ => "Testo"
            };
        }

        private bool FilterBySearch(Note note)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return true;

            var normalizedQuery = SearchQuery.ToLowerInvariant();
            return note.Title.ToLowerInvariant().Contains(normalizedQuery) ||
                   note.Content.ToLowerInvariant().Contains(normalizedQuery) ||
                   note.Tags.Any(tag => tag.ToLowerInvariant().Contains(normalizedQuery));
        }

        private bool FilterByType(Note note)
        {
            return FilterType switch
            {
                NoteFilterType.Favorites => note.IsFavorite,
                NoteFilterType.Recent => (DateTime.Now - note.ModifiedAt).TotalDays < 7,
                _ => true
            };
        }

        private bool FilterByTags(Note note)
        {
            if (!SelectedTags.Any())
                return true;

            return SelectedTags.All(tag => note.Tags.Contains(tag));
        }

        private async Task AddNewNoteAsync()
        {
            var newNote = new Note
            {
                Title = "Nuovo blocco appunti",
                Content = "",
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };
            Model.Notes.Add(newNote);
            Model.LastOpenedNoteId = newNote.Id;
            await DataModelService.SaveChangesAsync(_apiKey, Model);
            StateHasChanged();
            ToastService.Show("Nuova nota creata", ToastLevel.Success);
        }

        private void SetFilter(NoteFilterType filterType) => FilterType = filterType;

        private void ToggleTagFilter(string tag)
        {
            if (SelectedTags.Contains(tag))
                SelectedTags.Remove(tag);
            else
                SelectedTags.Add(tag);
        }

        private async Task ToggleFavorite()
        {
            if (SelectedNode != null)
            {
                SelectedNode.IsFavorite = !SelectedNode.IsFavorite;
                SelectedNode.ModifiedAt = DateTime.Now;

                var message = SelectedNode.IsFavorite ?
                    "Nota aggiunta ai preferiti" :
                    "Nota rimossa dai preferiti";

                await DataModelService.SaveChangesAsync(_apiKey, Model);
                ToastService.Show(message, ToastLevel.Success);
                StateHasChanged();
            }
        }

        private void DeleteNote()
        {
            if (SelectedNode != null)
            {
                _deleteModal.Show();
            }
        }

        private async Task ConfirmDelete()
        {
            if (SelectedNode != null)
            {
                Model.Notes.RemoveAll(n => n.Id == SelectedNode.Id);

                // Se la nota cancellata era quella attualmente selezionata, deseleziona
                if (Model.LastOpenedNoteId == SelectedNode.Id)
                {
                    Model.LastOpenedNoteId = null;
                }
                SelectedNode = null;

                await DataModelService.SaveChangesAsync(_apiKey, Model);
                ToastService.Show("Nota eliminata.", ToastLevel.Success);
                StateHasChanged();
            }
        }

        private async Task AddTag()
        {
            if (SelectedNode != null && !string.IsNullOrWhiteSpace(NewTag))
            {
                // Normalize tag
                var tag = NewTag.Trim().ToLowerInvariant();

                // Check if tag already exists
                if (!SelectedNode.Tags.Contains(tag))
                {
                    SelectedNode.Tags.Add(tag);
                    SelectedNode.ModifiedAt = DateTime.Now;
                    await DataModelService.SaveChangesAsync(_apiKey, Model);
                }

                NewTag = string.Empty;
            }
        }

        private async Task RemoveTag(string tag)
        {
            if (SelectedNode != null)
            {
                SelectedNode.Tags.Remove(tag);
                SelectedNode.ModifiedAt = DateTime.Now;
                await DataModelService.SaveChangesAsync(_apiKey, Model);
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