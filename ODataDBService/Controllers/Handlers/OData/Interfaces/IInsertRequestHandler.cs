// <copyright file="IInsertRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData.Interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Defines a contract for handling insert requests.
/// </summary>
public interface IInsertRequestHandler
{
    Task<IActionResult> HandleAsync(string tableName, JsonElement data);
}
