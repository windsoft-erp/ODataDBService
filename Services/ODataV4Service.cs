using ODataDBService.Models;
using ODataDBService.Repositories;
using System.Text.Json;

namespace ODataDBService.Services
{
    public class ODataV4Service : IODataV4Service
    {
        private readonly IODataV4Repository _oDataV4Repository;

        public ODataV4Service(IODataV4Repository oDataV4Repository)
        {
            _oDataV4Repository = oDataV4Repository ?? throw new ArgumentNullException(nameof(oDataV4Repository));
        }

        public async Task<ODataQueryResult> QueryAsync(string tableName, string select, string filter, string orderby, int top, int skip)
        {
            return await _oDataV4Repository.QueryAsync(tableName, select, filter, orderby, top, skip);
        }

        public async Task<bool> DeleteAsync(string tableName, string key)
        {
            return await _oDataV4Repository.DeleteAsync(tableName, key);
        }

        public async Task<bool> InsertAsync(string tableName, JsonElement data)
        {
            return await _oDataV4Repository.InsertAsync(tableName, data);
        }

        public async Task<bool> UpdateAsync(string tableName, string key, JsonElement data)
        {
            return await _oDataV4Repository.UpdateAsync(tableName, key, data);
        }
    }
}