namespace devbuddy.plugins.QueryOptimizer.Business.Basic
{
    public class SqlSelectColumn
    {
        public string Expression { get; set; }
        public string Alias { get; set; }
        public bool IsAsterisk { get; set; }
        public string TableName { get; set; } // Per colonne tipo "table.*"
    }
}
