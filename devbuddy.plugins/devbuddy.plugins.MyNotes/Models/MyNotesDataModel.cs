using devbuddy.common.Applications;

namespace devbuddy.plugins.MyNotes.Models
{
    public class MyNotesDataModel : CustomDataModelBase
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
    }

    public enum NoteType
    {
        Text,
        Code,
        Markdown
    }
}