using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.Services;
using devbuddy.plugins.MyNotes.Models;
using Microsoft.AspNetCore.Components;

namespace devbuddy.plugins.MyNotes
{
    public partial class Index : AppComponentBase<MyNotesDataModel>
    {
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
            await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
            StateHasChanged();
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

        private async Task DeleteNote(Note note)
        {
            Model.Notes.RemoveAll(n => n.Id == note.Id);

            // Se la nota cancellata era quella attualmente selezionata, deseleziona
            if (Model.LastOpenedNoteId == note.Id)
            {
                Model.LastOpenedNoteId = Model.Notes.FirstOrDefault()?.Id;
            }

            await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
        }
    }
}