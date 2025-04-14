using System.Text.RegularExpressions;
using devbuddy.plugins.Notes.Models;
using Microsoft.AspNetCore.Components;

namespace devbuddy.plugins.Notes
{
    public enum NoteFilterType
    {
        All,
        Favorites,
        Recent
    }

    public partial class NotesList
    {
        [Parameter]
        public List<Note> Notes { get; set; } = [];

        [Parameter]
        public Note? SelectedNote { get; set; }

        [Parameter]
        public EventCallback<Note> OnNoteSelected { get; set; }

        [Parameter]
        public EventCallback<Note> OnNoteAdded { get; set; }

        private string SearchQuery { get; set; } = string.Empty;
        private NoteFilterType FilterType { get; set; } = NoteFilterType.All;
        private List<string> SelectedTags { get; set; } = [];

        private List<Note> FilteredNotes => Notes
            .Where(note => FilterBySearch(note))
            .Where(note => FilterByType(note))
            .Where(note => FilterByTags(note))
            .OrderByDescending(note => note.ModifiedAt)
            .ToList();

        private List<string> AllTags => Notes
            .SelectMany(note => note.Tags)
            .Distinct()
            .OrderBy(tag => tag)
            .ToList();

        private string GetNoteTypeIconClass(NoteType type) => type switch
        {
            NoteType.Code => "fa-solid fa-code",
            NoteType.Markdown => "fa-brands fa-markdown",
            _ => "fa-solid fa-file-lines"
        };

        private string GetNoteTypeTitle(NoteType type) => type switch
        {
            NoteType.Code => "Codice",
            NoteType.Markdown => "Markdown",
            _ => "Testo"
        };

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

        private string GetPreview(Note note)
        {
            var content = note.Content;

            // Rimuovi i tag HTML/Markdown 
            var strippedContent = Regex.Replace(content, "<.*?>", string.Empty);
            strippedContent = Regex.Replace(strippedContent, "#{1,6}\\s?", string.Empty);

            // Limita la lunghezza
            if (strippedContent.Length > 100)
                return strippedContent.Substring(0, 97) + "...";

            return strippedContent;
        }

        private async Task SelectNote(Note note)
        {
            SelectedNote = note;
            await OnNoteSelected.InvokeAsync(note);
        }

        private async Task AddNewNote()
        {
            var newNote = new Note
            {
                Title = "Nuova Nota",
                Content = "",
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            await OnNoteAdded.InvokeAsync(newNote);
        }

        private void SetFilter(NoteFilterType filterType)
        {
            FilterType = filterType;
        }

        private void ToggleTagFilter(string tag)
        {
            if (SelectedTags.Contains(tag))
                SelectedTags.Remove(tag);
            else
                SelectedTags.Add(tag);
        }
    }
}