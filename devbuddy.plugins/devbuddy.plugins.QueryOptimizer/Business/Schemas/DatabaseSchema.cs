namespace devbuddy.plugins.QueryOptimizer.Business.Schemas
{

    public class DatabaseSchema
    {
        public Dictionary<string, TableSchema> Tables { get; set; } = [];
        public string DatabaseType { get; set; }
    }
}
