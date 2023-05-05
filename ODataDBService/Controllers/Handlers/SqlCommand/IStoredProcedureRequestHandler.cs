// <copyright file="IStoredProcedureRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Controllers.Handlers.SqlCommand;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Interface for handling stored procedure requests.
/// </summary>
public interface IStoredProcedureRequestHandler
{
    Task<IActionResult> HandleAsync(string storedProcedureName, JsonElement procedureParameters);
}
