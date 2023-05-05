// <copyright file="ODataV4Service.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services;
using System.Text.Json;
using Models;
using Repositories;

/// <summary>
/// Provides a service to query and manipulate data for an OData V4 endpoint.
/// </summary>
public class ODataV4Service : IODataV4Service
{
    private readonly IODataV4Repository oDataV4Repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataV4Service"/> class.
    /// </summary>
    /// <param name="oDataV4Repository">The repository to retrieve data from.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="oDataV4Repository"/> is null.</exception>
    public ODataV4Service(IODataV4Repository oDataV4Repository)
    {
        this.oDataV4Repository = oDataV4Repository ?? throw new ArgumentNullException(nameof(oDataV4Repository));
    }

    /// <summary>
    /// Queries the table with the provided query parameters and returns the result.
    /// </summary>
    /// <param name="query">The query parameters to use.</param>
    /// <returns>The query result.</returns>
    public async Task<ODataQueryResult> QueryAsync(ODataQuery query)
    {
        var rows = await this.oDataV4Repository.QueryAsync(query);
        var resultList = rows.ToList();
        var count = resultList.Count();
        var result = new ODataQueryResult
        {
            Count = count,
            Value = resultList,
        };

        return result;
    }

    /// <summary>
    /// Queries a record by ID from the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="key">The ID of the record to query.</param>
    /// <returns>The queried record.</returns>
    public async Task<dynamic> QueryByIdAsync(string tableName, string key)
    {
        return await this.oDataV4Repository.QueryByIdAsync(tableName, key) ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Deletes a record from the specified table by its ID.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="key">The ID of the record to delete.</param>
    /// <returns>True if the record was deleted, false otherwise.</returns>
    public async Task<bool> DeleteAsync(string tableName, string key)
    {
        return await this.oDataV4Repository.DeleteAsync(tableName, key);
    }

    /// <summary>
    /// Inserts a new record into the specified table with the provided data.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="data">The data for the new record.</param>
    /// <returns>The inserted record.</returns>
    public async Task<dynamic?> InsertAsync(string tableName, JsonElement data)
    {
        var result = await this.oDataV4Repository.InsertAsync(tableName, data);
        var insertedItem = await this.oDataV4Repository.QueryByExtractedIdAsync(tableName, data);
        return result switch
        {
            true => insertedItem ?? data.Deserialize<dynamic>(),
            false => null,
        };
    }

    /// <summary>
    /// Updates the record in the specified table with the provided ID and data.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="key">The ID of the record to update.</param>
    /// <param name="data">The updated data for the record.</param>
    /// <returns>True if the record was updated, false otherwise.</returns>
    public async Task<bool> UpdateAsync(string tableName, string key, JsonElement data)
    {
        return await this.oDataV4Repository.UpdateAsync(tableName, key, data);
    }
}
