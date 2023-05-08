// <copyright file="InvalidateCacheRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Controllers.Handlers.OData;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Repositories;
using static Utilities.Constants.RequestHandlerConstants;

/// <summary>
/// Handles the request to invalidate the cache of the table information for a specific table.
/// </summary>
public class InvalidateCacheRequestHandler : BaseRequestHandler, IInvalidateCacheRequestHandler
{
    private readonly IODataV4Repository repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidateCacheRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="repository">The repository.</param>
    public InvalidateCacheRequestHandler(ILogger<InvalidateCacheRequestHandler> logger, IODataV4Repository repository)
        : base(logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Invalidates the cache for a specific table and returns an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="tableName">The name of the table whose cache should be invalidated.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a success message if the cache was invalidated successfully,
    /// or a "not found" message if the specified table was not found in the cache.
    /// </returns>
    public IActionResult Handle(string tableName)
    {
        var result = this.repository.InvalidateTableInfoCache(tableName);

        return result
            ? this.HandleSuccess(string.Format(InvalidateCacheSuccessMessageFormat, tableName))
            : this.HandleNotFound(string.Format(InvalidateCacheNotFoundMessageFormat, tableName));
    }
}