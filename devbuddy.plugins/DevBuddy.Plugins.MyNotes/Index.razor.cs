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

            // Se non ci sono note, crea una nota di esempio
            if (!Model.Notes.Any())
            {
                await CreateSampleNotes();
            }

            await base.OnInitializedAsync();
        }

        private async Task CreateSampleNotes()
        {
            // Crea una nota di benvenuto in Markdown
            var welcomeNote = new Note
            {
                Title = "Benvenuto in MyNotes!",
                Content = "# Benvenuto in MyNotes!\n\nQuesto plugin ti permette di salvare e organizzare:\n\n" +
                          "- Appunti e promemoria\n" +
                          "- Snippet di codice\n" +
                          "- Documentazione in Markdown\n\n" +
                          "## Funzionalità principali\n\n" +
                          "- Organizza le note con tag\n" +
                          "- Salva snippet di codice con supporto per vari linguaggi\n" +
                          "- Cerca rapidamente tra tutte le tue note\n" +
                          "- Segna le note importanti come preferite\n\n" +
                          "Inizia creando una nuova nota con il pulsante + in alto a destra!",
                Type = NoteType.Markdown,
                IsFavorite = true,
                Tags = new List<string> { "tutorial", "markdown" }
            };

            // Crea uno snippet di codice C# di esempio
            var codeNote = new Note
            {
                Title = "Esempio di snippet C#",
                Content = "// Esempio di classe in C#\npublic class Person\n{\n    public string Name { get; set; }\n    public int Age { get; set; }\n\n" +
                          "    public Person(string name, int age)\n    {\n        Name = name;\n        Age = age;\n    }\n\n" +
                          "    public override string ToString()\n    {\n        return $\"{Name}, {Age} anni\";\n    }\n}",
                Type = NoteType.Code,
                Language = "csharp",
                Tags = new List<string> { "csharp", "esempio", "snippet" }
            };

            // Aggiungi le note di esempio al modello
            Model.Notes.Add(welcomeNote);
            Model.Notes.Add(codeNote);

            // Imposta la nota di benvenuto come selezionata
            Model.LastOpenedNoteId = welcomeNote.Id;

            // Salva le modifiche
            await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
        }

        private async Task SelectNote(Note note)
        {
            Model.LastOpenedNoteId = note.Id;
            await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
        }

        private async Task AddNote(Note note)
        {
            Model.Notes.Add(note);
            Model.LastOpenedNoteId = note.Id;
            await DataModelService.AddOrUpdateAsync(nameof(MyNotes), Model);
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