namespace ODataDBService.Services
{
    public interface ISQLCommandService
    {
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object> parameters);
    }
}
