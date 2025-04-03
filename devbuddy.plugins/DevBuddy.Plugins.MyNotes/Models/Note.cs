using devbuddy.common.Models;

namespace devbuddy.Plugins.MyNotes.Models
{
    public class Note
    {
        public string Value { get; set; }
        public bool IsSecret { get; set; }
        public DateTime AddedOn { get; set; } = DateTime.Now;
        public bool View { get; set; } = false;
    }


    public static class DictionaryExtensions
    {
        public static PaginationResult<int, (string value, bool isSecret)> Paginate(
            this Dictionary<int, (string value, bool isSecret)> dictionary,
            int pageNumber,
            int pageSize)
        {
            // Validazione input
            if (pageNumber < 1) throw new ArgumentException("Il numero di pagina deve essere maggiore di 0", nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentException("La dimensione della pagina deve essere maggiore di 0", nameof(pageSize));

            var totalItems = dictionary.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Assicurati che pageNumber non superi il numero totale di pagine
            pageNumber = Math.Min(pageNumber, totalPages);

            // Ottieni gli elementi per la pagina corrente
            var items = dictionary
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToDictionary(x => x.Key, x => x.Value);

            return new PaginationResult<int, (string value, bool isSecret)>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }
    }
}
