using ODataDBService.Models;
using System.Text.Json;

namespace ODataDBService.Services.Repositories
{
    public interface IODataV4Repository
    {
        Task<IEnumerable<dynamic>> QueryAsync(ODataQuery oDataQuery);
        Task<bool> DeleteAsync(string tableName, string key);
        Task<bool> InsertAsync(string tableName, JsonElement data);
        Task<bool> UpdateAsync(string tableName, string key, JsonElement data);
        bool InvalidateTableInfoCache(string tableName);    
    }
}