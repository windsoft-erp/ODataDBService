using ODataDBService.Models;

namespace ODataDBService.Services.Repositories
{
    public interface ISQLCommandRepository
    {
        Task<IEnumerable<StoredProcedureParameter>> GetStoredProcedureParametersAsync(string storedProcedureName);
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object>? parameters = null);
    }
}
