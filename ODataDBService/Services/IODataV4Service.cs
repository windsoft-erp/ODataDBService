// <copyright file="IODataV4Service.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services;
using System.Text.Json;
using Models;

/// <summary>
/// Interface for a service that handles OData v4 queries.
/// </summary>
public interface IODataV4Service
{
    Task<ODataQueryResult> QueryAsync(ODataQuery query);

    Task<dynamic> QueryByIdAsync(string tableName, string key);

    Task<bool> DeleteAsync(string tableName, string key);

    Task<dynamic?> InsertAsync(string tableName, JsonElement data);

    Task<bool> UpdateAsync(string tableName, string key, JsonElement data);
}
