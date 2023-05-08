// <copyright file="ODataV4Repository.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services.Repositories;

using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;
using Dapper;
using DynamicODataToSQL;
using Models;
using Utilities.Extensions;

/// <summary>
/// Represents a repository for handling OData v4 requests and responses.
/// </summary>
public class ODataV4Repository : IODataV4Repository
{
    private readonly string connectionString;
    private readonly IODataToSqlConverter oDataToSqlConverter;
    private readonly ConcurrentDictionary<string, TableInfo> tableInfoCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataV4Repository"/> class.
    /// </summary>
    /// <param name="configuration">The configuration used to get the connection string.</param>
    /// <param name="oDataToSqlConverter">The converter used to convert OData queries to SQL queries.</param>
    public ODataV4Repository(IConfiguration configuration, IODataToSqlConverter oDataToSqlConverter)
    {
        this.oDataToSqlConverter = oDataToSqlConverter ?? throw new ArgumentNullException(nameof(oDataToSqlConverter));
        this.connectionString = configuration?.GetConnectionString("Sql") ?? throw new ArgumentNullException(nameof(configuration));
        this.tableInfoCache = new ConcurrentDictionary<string, TableInfo>();
    }

    /// <summary>
    /// Queries the database using the specified OData query.
    /// </summary>
    /// <param name="oDataQuery">The OData query to execute.</param>
    /// <returns>The result of the query as a dynamic object.</returns>
    public async Task<IEnumerable<dynamic>> QueryAsync(ODataQuery oDataQuery)
    {
        var (query, queryParams) = this.oDataToSqlConverter.ConvertToSQL(
                oDataQuery.TableName,
                new Dictionary<string, string>
                {
                    { "select", oDataQuery.Select },
                    { "filter", oDataQuery.Filter },
                    { "apply", oDataQuery.Apply },
                    { "orderby", oDataQuery.OrderBy },
                    { "top", (oDataQuery.Top + 1).ToString() },
                    { "skip", oDataQuery.Skip.ToString() },
                });

        await using var conn = new SqlConnection(this.connectionString);
        var rows = await conn.QueryAsync<dynamic>(query, queryParams).ConfigureAwait(false);

        return rows;
    }

    /// <summary>
    /// Queries the database for a record with the same primary key as the given data, if available.
    /// </summary>
    /// <param name="tableName">The name of the table to query.</param>
    /// <param name="data">The JSON data containing the primary key to search for.</param>
    /// <returns>
    /// A <c>dynamic</c> object representing the database record with the matching primary key,
    /// or <c>null</c> if no record is found or the primary key is null or missing from the data.
    /// </returns>
    public async Task<dynamic?> QueryByExtractedIdAsync(string tableName, JsonElement data)
    {
        try
        {
            var tableInfo = this.GetTableInfo(tableName);

            if (tableInfo.PrimaryKey != null && data.TryGetProperty(tableInfo.PrimaryKey, out var primaryKeyValue) &&
                primaryKeyValue.ValueKind != JsonValueKind.Null)
            {
                // If the primary key is included in the data and is not null, use it instead of the passed in key
                return await this.QueryByIdAsync(tableName, primaryKeyValue.ToString());
            }
        }
        catch (ArgumentException ex) when (ex.Message == $"Could not find primary key for table {tableName}")
        {
            // Ignoring this exception as there is no primary key for the given table
            // and this is expected behavior in some cases.
        }

        return null;
    }

    /// <summary>
    /// Queries the database for a record with the specified primary key value.
    /// </summary>
    /// <param name="tableName">The name of the table to query.</param>
    /// <param name="id">The value of the primary key.</param>
    /// <returns>The result of the query as a dynamic object.</returns>
    public async Task<dynamic?> QueryByIdAsync(string tableName, string id)
    {
        var tableInfo = this.GetTableInfo(tableName);

        var sql = $"SELECT * FROM {tableInfo.TableName} WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}";
        var parameters = new DynamicParameters();
        parameters.Add(tableInfo.PrimaryKey, id);

        await using var conn = new SqlConnection(this.connectionString);
        var result = await conn.QueryAsync<dynamic>(sql, parameters).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Deletes a record from the database with the specified primary key value.
    /// </summary>
    /// <param name="tableName">The name of the table to delete from.</param>
    /// <param name="key">The value of the primary key.</param>
    /// <returns>True if the record was deleted, otherwise false.</returns>
    public async Task<bool> DeleteAsync(string tableName, string key)
    {
        var tableInfo = this.GetTableInfo(tableName);

        var sql = $"DELETE FROM {tableInfo.TableName} WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}";
        var parameters = new DynamicParameters();
        parameters.Add(tableInfo.PrimaryKey, key);

        await using var conn = new SqlConnection(this.connectionString);
        var result = await conn.ExecuteAsync(sql, parameters).ConfigureAwait(false);

        return result > 0;
    }

    /// <summary>
    /// Inserts a new record into the database.
    /// </summary>
    /// <param name="tableName">The name of the table to insert into.</param>
    /// <param name="data">The data to insert.</param>
    /// <returns>True if the record was inserted, otherwise false.</returns>
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

        await using var conn = new SqlConnection(this.connectionString);
        var result = await conn.ExecuteAsync(sql, parameters).ConfigureAwait(false);

        return result > 0;
    }

    /// <summary>
    /// Updates a record in the database with the specified primary key value.
    /// </summary>
    /// <param name="tableName">The name of the table to update.</param>
    /// <param name="key">The value of the primary key.</param>
    /// <param name="data">The data to update.</param>
    /// <returns>True if the record was updated, otherwise false.</returns>
    public async Task<bool> UpdateAsync(string tableName, string key, JsonElement data)
    {
        var properties = data.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value);
        var tableInfo = this.GetTableInfo(tableName);

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

        await using var conn = new SqlConnection(this.connectionString);
        var result = await conn.ExecuteAsync(sb.ToString(), queryParams).ConfigureAwait(false);

        return result > 0;
    }

    /// <summary>
    /// Invalidates the cache for the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table to invalidate the cache for.</param>
    /// <returns>True if the cache was invalidated, otherwise false.</returns>
    public bool InvalidateTableInfoCache(string tableName)
    {
        return this.tableInfoCache.TryRemove(tableName, out _);
    }

    private TableInfo GetTableInfo(string tableName)
    {
        if (this.tableInfoCache.TryGetValue(tableName, out var cachedTableInfo))
        {
            return cachedTableInfo;
        }

        using var connection = new SqlConnection(this.connectionString);

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

        this.tableInfoCache.TryAdd(tableName, tableInfo);
        return tableInfo;
    }
}