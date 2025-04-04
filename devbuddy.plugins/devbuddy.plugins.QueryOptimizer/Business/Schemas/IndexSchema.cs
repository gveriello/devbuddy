namespace devbuddy.plugins.QueryOptimizer.Business.Schemas
{
    public class IndexSchema
    {
        public string Name { get; set; }
        public List<string> Columns { get; set; } = new List<string>();
        public bool IsUnique { get; set; }
        public bool IsPrimary { get; set; }
    }
}
