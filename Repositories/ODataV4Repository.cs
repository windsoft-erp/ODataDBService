using Dapper;
using DynamicODataToSQL;
using ODataDBService.Models;
using System.Data.SqlClient;
using System.Text.Json;
using ODataDBService.Extensions;
using System.Text;

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
            var tableInfo = GetTableInfo(tableName);

            var sql = $"DELETE FROM {tableInfo.TableName} WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}";
            var parameters = new DynamicParameters();
            parameters.Add(tableInfo.PrimaryKey, key);

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(sql, parameters).ConfigureAwait(false);

            return result > 0;
        }

        public async Task<bool> InsertAsync(string tableName, JsonElement data)
        {
            var properties = data.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => prop.Value);

            var columnNames = string.Join(", ", properties.Keys);
            var valueParams = string.Join(", ", properties.Keys.Select(key => $"@{key}"));

            var sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({valueParams})";
            var parameters = new DynamicParameters();

            foreach (var (key, value) in properties)
            {
                parameters.Add(key, value.ToObject());
            }

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(sql, parameters).ConfigureAwait(false);

            return result > 0;
        }

        public async Task<bool> UpdateAsync(string tableName, string key, JsonElement data)
        {
            var properties = data.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value);
            var tableInfo = GetTableInfo(tableName);

            var queryParams = new DynamicParameters();

            var sb = new StringBuilder($"UPDATE {tableInfo.TableName} SET ");
            foreach (var property in properties)
            {
                var propertyName = property.Key;
                var propertyValue = property.Value;

                if (propertyName.Equals(tableInfo.PrimaryKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (tableInfo.ColumnNames?.Contains(propertyName, StringComparer.OrdinalIgnoreCase) != true)
                {
                    throw new ArgumentException($"Column {propertyName} does not exist in table {tableName}");
                }

                sb.Append($"{propertyName} = @{propertyName}, ");
                queryParams.Add(propertyName, propertyValue.ToObject());
            }

            sb.Remove(sb.Length - 2, 2); // remove last comma and space
            sb.Append($" WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}");
            queryParams.Add(tableInfo.PrimaryKey, key);

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(sb.ToString(), queryParams).ConfigureAwait(false);

            return result > 0;
        }

        private TableInfo GetTableInfo(string tableName)
        {
            using var connection = new SqlConnection(_connectionString);

            var tableInfo = new TableInfo
            {
                TableName = tableName,
                PrimaryKey = connection.QueryFirstOrDefault<string>(
                    @"SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                    WHERE TABLE_NAME = @TableName 
                    AND CONSTRAINT_NAME LIKE 'PK%'",
                    new { TableName = tableName })
            };

            if (tableInfo.PrimaryKey == null)
            {
                throw new ArgumentException($"Could not find primary key for table {tableName}");
            }

            tableInfo.ColumnNames = connection.Query<string>(
                @"SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @TableName",
                new { TableName = tableName });

            return tableInfo;
        }
    }
}
