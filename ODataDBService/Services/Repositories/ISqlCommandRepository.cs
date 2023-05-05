// <copyright file="ISqlCommandRepository.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services.Repositories;
using Models;

/// <summary>
/// Interface for a repository that handles sql commands.
/// </summary>
public interface ISqlCommandRepository
{
    Task<IEnumerable<StoredProcedureParameter>> GetStoredProcedureParametersAsync(string storedProcedureName);

    Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object?>? parameters = null);
}