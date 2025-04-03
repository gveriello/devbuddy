namespace devbuddy.common.Models
{
    public class PaginationResult<TKey, TValue>
    {
        public IEnumerable<KeyValuePair<TKey, TValue>> Items { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
