using ODataDBService.Models;

namespace ODataDBService.Services.Repositories;

public interface ISqlCommandRepository
{
    Task<IEnumerable<StoredProcedureParameter>> GetStoredProcedureParametersAsync(string storedProcedureName);
    Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object>? parameters = null);
}

