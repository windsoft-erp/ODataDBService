using Dapper;
using DynamicODataToSQL;
using ODataDBService.Models;
using System.Data.SqlClient;
using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using ODataDBService.Services.Extensions;

namespace ODataDBService.Services.Repositories;

public class ODataV4Repository : IODataV4Repository
{
    private readonly string _connectionString;
    private readonly IODataToSqlConverter _oDataToSqlConverter;
    private readonly ConcurrentDictionary<string, TableInfo> _tableInfoCache;

    public ODataV4Repository(IConfiguration configuration, IODataToSqlConverter oDataToSqlConverter)
    {
        _oDataToSqlConverter = oDataToSqlConverter ?? throw new ArgumentNullException(nameof(oDataToSqlConverter));
        _connectionString = configuration?.GetConnectionString("Sql") ?? throw new ArgumentNullException(nameof(configuration));
        _tableInfoCache = new ConcurrentDictionary<string, TableInfo>();
    }

    public async Task<IEnumerable<dynamic>> QueryAsync(ODataQuery oDataQuery)
    {
        var (query, queryParams) = _oDataToSqlConverter.ConvertToSQL(oDataQuery.TableName,
                new Dictionary<string, string>
                {
                    { "select", oDataQuery.Select },
                    { "filter", oDataQuery.Filter },
                    { "apply", oDataQuery.Apply },
                    { "orderby", oDataQuery.OrderBy },
                    { "top", (oDataQuery.Top + 1).ToString() },
                    { "skip", oDataQuery.Skip.ToString() },
                }
            );

        await using var conn = new SqlConnection(_connectionString);
        var rows = await conn.QueryAsync<dynamic>(query, queryParams).ConfigureAwait(false);

        return rows;
    }
    
    public async Task<dynamic?> QueryByExtractedIdAsync(string tableName, JsonElement data)
    {
        var tableInfo = GetTableInfo(tableName);

        if (data.TryGetProperty(tableInfo.PrimaryKey, out var primaryKeyValue) && primaryKeyValue.ValueKind != JsonValueKind.Null)
        {
            // If the primary key is included in the data and is not null, use it instead of the passed in key
            return await QueryByIdAsync(tableName, primaryKeyValue.ToString());
        }

        return null;
    }
    
    public async Task<dynamic?> QueryByIdAsync(string tableName, string id)
    {
        var tableInfo = GetTableInfo(tableName);

        var sql = $"SELECT * FROM {tableInfo.TableName} WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}";
        var parameters = new DynamicParameters();
        parameters.Add(tableInfo.PrimaryKey, id);

        await using var conn = new SqlConnection(_connectionString);
        var result = await conn.QueryAsync<dynamic>(sql, parameters).ConfigureAwait(false);

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

    public bool InvalidateTableInfoCache(string tableName)
    {
        return _tableInfoCache.TryRemove(tableName, out _);
    }

    private TableInfo GetTableInfo(string tableName)
    {
        if (_tableInfoCache.TryGetValue(tableName, out var cachedTableInfo))
        {
            return cachedTableInfo;
        }

        using var connection = new SqlConnection(_connectionString);

        var tableInfo = new TableInfo { TableName = tableName };

        var sql = @"
            SELECT COLUMN_NAME, CONSTRAINT_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE TABLE_NAME = @TableName;
    
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @TableName;
        ";

        using (var multi = connection.QueryMultiple(sql, new { TableName = tableName }))
        {
            var keyColumnUsage = multi.Read<dynamic>().ToList();
            var columns = multi.Read<string>().ToList();

            tableInfo.PrimaryKey = keyColumnUsage
                .Where(x => ((string)x.CONSTRAINT_NAME).StartsWith("PK"))
                .Select(x => (string)x.COLUMN_NAME)
                .FirstOrDefault();

            if (tableInfo.PrimaryKey == null)
            {
                throw new ArgumentException($"Could not find primary key for table {tableName}");
            }

            tableInfo.ColumnNames = columns;
        }

        _tableInfoCache.TryAdd(tableName, tableInfo);
        return tableInfo;
    }
}
