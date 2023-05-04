using ODataDBService.Models;
using ODataDBService.Services.Repositories;
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

        public async Task<ODataQueryResult> QueryAsync(ODataQuery query)
        {
            var rows = await _oDataV4Repository.QueryAsync(query);
            var resultList = rows.ToList();
            var count = resultList.Count();
            var result = new ODataQueryResult
            {
                Count = count,
                Value = resultList
            };

            return result;
        }

        public async Task<dynamic> QueryByIdAsync(string tableName, string key)
        {
            return await _oDataV4Repository.QueryByIdAsync(tableName, key);
        }

        public async Task<bool> DeleteAsync(string tableName, string key)
        {
            return await _oDataV4Repository.DeleteAsync(tableName, key);
        }

        public async Task<dynamic?> InsertAsync(string tableName, JsonElement data)
        {
            var result = await _oDataV4Repository.InsertAsync(tableName, data);
            return result switch
            {
                true => await _oDataV4Repository.QueryByExtractedIdAsync(tableName, data),
                false => null
            };
        }

        public async Task<bool> UpdateAsync(string tableName, string key, JsonElement data)
        {
            return await _oDataV4Repository.UpdateAsync(tableName, key, data);
        }

        public void InvalidateTableInfoCache(string tableName) 
        {
            _oDataV4Repository.InvalidateTableInfoCache(tableName);    
        }
    }
}