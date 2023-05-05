﻿// <copyright file="IBatchRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData.Interfaces;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Defines a contract for handling batch requests.
/// </summary>
public interface IBatchRequestHandler
{
    Task<IActionResult> ProcessBatchRequestAsync(HttpRequest request);
}
