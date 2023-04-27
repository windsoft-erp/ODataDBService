using Dapper;
using DynamicODataToSQL;
using ODataDBService.Models;
using System.Data.SqlClient;
using System.Text.Json;
using ODataDBService.Extensions;

namespace ODataDBService.Repositories
{
    public class ODataV4Repository : IODataV4Repository
    {
        private readonly string _connectionString;
        private readonly IODataToSqlConverter _oDataToSqlConverter;

        public ODataV4Repository(IConfiguration configuration, IODataToSqlConverter oDataToSqlConverter)
        {
            _oDataToSqlConverter = oDataToSqlConverter ?? throw new ArgumentNullException(nameof(oDataToSqlConverter));
            _connectionString = configuration?.GetConnectionString("Sql") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<ODataQueryResult> QueryAsync(string tableName, string select, string filter, string orderby, int top, int skip)
        {
            var (query, queryParams) = _oDataToSqlConverter.ConvertToSQL(tableName,
                    new Dictionary<string, string>
                    {
                        { "select", select },
                        { "filter", filter },
                        { "orderby", orderby },
                        { "top", (top + 1).ToString() },
                        { "skip", skip.ToString() }
                    }
                );

            await using var conn = new SqlConnection(_connectionString);
            var rows = await conn.QueryAsync<dynamic>(query, queryParams).ConfigureAwait(false);

            var resultList = rows.ToList();
            var count = resultList.Count();
            var result = new ODataQueryResult
            {
                Count = count,
                Value = resultList
            };

            return result;
        }

        public async Task<bool> DeleteAsync(string tableName, string key)
        {
            var (query, queryParams) = _oDataToSqlConverter.ConvertToSQLDelete(tableName, key, _connectionString);

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(query, queryParams).ConfigureAwait(false);

            return result > 0;
        }

        public async Task<bool> InsertAsync(string tableName, JsonElement data)
        {
            var properties = data.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => prop.Value);

            var (query, queryParams) = _oDataToSqlConverter.ConvertToSQLInsert(tableName, properties);

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(query, queryParams).ConfigureAwait(false);

            return result > 0;
        }

        public async Task<bool> UpdateAsync(string tableName, string key, JsonElement data)
        {
            var properties = data.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value);

            var (query, queryParams) = _oDataToSqlConverter.ConvertToSQLUpdate(tableName, key, properties, _connectionString);

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(query, queryParams).ConfigureAwait(false);

            return result > 0;
        }
    }
}
