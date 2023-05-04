namespace ODataDBService.Services;

public interface ISqlCommandService
{
    Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object> parameters);
}

