using ODataDBService.Models;
using System.Text.Json;

namespace ODataDBService.Services
{
    public interface IODataV4Service
    {
        Task<ODataQueryResult> QueryAsync(ODataQuery query);
        Task<bool> DeleteAsync(string tableName, string key);
        Task<bool> InsertAsync(string tableName, JsonElement data);
        Task<bool> UpdateAsync(string tableName, string key, JsonElement data);
        void InvalidateTableInfoCache(string tableName);
    }
}
