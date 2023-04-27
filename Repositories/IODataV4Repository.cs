﻿using ODataDBService.Models;
using System.Text.Json;

namespace ODataDBService.Repositories
{
    public interface IODataV4Repository
    {
        Task<ODataQueryResult> QueryAsync(string tableName, string select, string filter, string orderby, int top, int skip);
        Task<bool> DeleteAsync(string tableName, string key);
        Task<bool> InsertAsync(string tableName, JsonElement data);
        Task<bool> UpdateAsync(string tableName, string key, JsonElement data);
    }
}