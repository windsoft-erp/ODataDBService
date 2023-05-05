// <copyright file="ISqlCommandService.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services;

/// <summary>
/// Represents a service that can execute stored procedures in a SQL database.
/// </summary>
public interface ISqlCommandService
{
    Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object?> parameters);
}