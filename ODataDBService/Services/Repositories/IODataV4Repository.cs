// <copyright file="IODataV4Repository.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services.Repositories;
using System.Text.Json;
using Models;

/// <summary>
/// Interface for a repository that handles OData v4 queries.
/// </summary>
public interface IODataV4Repository
{
    Task<IEnumerable<dynamic>> QueryAsync(ODataQuery oDataQuery);

    Task<dynamic?> QueryByIdAsync(string tableName, string key);

    Task<dynamic?> QueryByExtractedIdAsync(string tableName, JsonElement data);

    Task<bool> DeleteAsync(string tableName, string key);

    Task<bool> InsertAsync(string tableName, JsonElement data);

    Task<bool> UpdateAsync(string tableName, string key, JsonElement data);

    bool InvalidateTableInfoCache(string tableName);
}
