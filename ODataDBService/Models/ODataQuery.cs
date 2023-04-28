namespace ODataDBService.Models
{
    public class ODataQuery
    {
        public string TableName { get; set; } = string.Empty;
        public string Select { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;
        public string Apply { get; set; } = string.Empty;
        public string OrderBy { get; set; } = string.Empty;
        public int Top { get; set; }
        public int Skip { get; set; }
    }
}
