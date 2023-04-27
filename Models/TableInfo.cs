namespace ODataDBService.Models
{
    public class TableInfo
    {
        public string? TableName { get; set; }
        public string? PrimaryKey { get; set; }
        public IEnumerable<string>? ColumnNames { get; set; }
    }
}
