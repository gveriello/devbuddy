using System.Text.RegularExpressions;
using devbuddy.common.Applications;

namespace devbuddy.plugins.Notes.Models
{
    public class NotesDataModel : CustomDataModelBase
    {
        public List<Note> Notes { get; set; } = [];
        public string? LastOpenedNoteId { get; set; }
    }

    public class Note
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "Nuova Nota";
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
        public string Language { get; set; } = "plaintext";
        public bool IsFavorite { get; set; } = false;
        public List<string> Tags { get; set; } = [];

        NoteType _type = NoteType.Text;
        public NoteType Type
        {
            get => _type;
            set
            {
                _type = value;
                //OnTypeChanged();
            }
        }

        public string GetPreview()
        {
            var content = Content;

            // Rimuovi i tag HTML/Markdown 
            var strippedContent = Regex.Replace(content, "<.*?>", string.Empty);
            strippedContent = Regex.Replace(strippedContent, "#{1,6}\\s?", string.Empty);

            // Limita la lunghezza
            if (strippedContent.Length > 100)
                return string.Concat(strippedContent.AsSpan(0, 97), "...");

            return strippedContent;
        }
    }

    public enum NoteType
    {
        Text,
        Code,
        Markdown
    }
}