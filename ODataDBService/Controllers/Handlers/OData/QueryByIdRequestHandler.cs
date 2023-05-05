// <copyright file="QueryByIdRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services;

/// <summary>
/// Handles OData query by ID requests.
/// </summary>
/// <seealso cref="BaseRequestHandler"/>
/// <seealso cref="IQueryByIdRequestHandler"/>
public class QueryByIdRequestHandler : BaseRequestHandler, IQueryByIdRequestHandler
{
    private readonly IODataV4Service oDataV4Service;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryByIdRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="oDataV4Service">The OData V4 service.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="oDataV4Service"/> is null.</exception>
    public QueryByIdRequestHandler(ILogger<QueryByIdRequestHandler> logger, IODataV4Service oDataV4Service)
        : base(logger)
    {
        this.oDataV4Service = oDataV4Service ?? throw new ArgumentNullException(nameof(oDataV4Service));
    }

    /// <summary>
    /// Handles a query-by-ID request for the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table to query.</param>
    /// <param name="key">The key of the record to query.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the query.</returns>
    public async Task<IActionResult> HandleAsync(string tableName, string key)
    {
        try
        {
            var result = await this.oDataV4Service.QueryByIdAsync(tableName, key);

            return result switch
            {
                not null => this.HandleCreated($"Successfully got record from table '{tableName}'.", result),
                _ => this.HandleNotFound($"Error getting record from table '{tableName}'."),
            };
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error getting record from table '{tableName}'.";
            return this.HandleError(errorMessage, ex);
        }
    }
}
