namespace ODataDBService.Models;

public class ODataQuery
{
    public string TableName { get; init; } = string.Empty;
    public string Select { get; init; } = string.Empty;
    public string Filter { get; init; } = string.Empty;
    public string Apply { get; init; } = string.Empty;
    public string OrderBy { get; init; } = string.Empty;
    public int Top { get; init; }
    public int Skip { get; init; }
}

