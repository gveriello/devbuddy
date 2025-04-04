namespace devbuddy.plugins.QueryOptimizer.Business.Schemas
{
    public class TableSchema
    {
        public string Name { get; set; }
        public List<ColumnSchema> Columns { get; set; } = [];
        public List<IndexSchema> Indices { get; set; } = [];
    }
}
