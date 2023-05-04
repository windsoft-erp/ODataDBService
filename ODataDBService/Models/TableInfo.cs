namespace ODataDBService.Models;
public class TableInfo
{
    public string? TableName { get; init; }
    public string? PrimaryKey { get; set; }
    public IEnumerable<string>? ColumnNames { get; set; }
}

