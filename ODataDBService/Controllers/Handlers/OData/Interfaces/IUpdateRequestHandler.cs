// <copyright file="IUpdateRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData.Interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Defines a contract for handling an update request.
/// </summary>
public interface IUpdateRequestHandler
{
    Task<IActionResult> HandleAsync(string tableName, string key, JsonElement data);
}
