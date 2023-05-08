// <copyright file="QueryByIdRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Controllers.Handlers.OData;
using System.Data.SqlClient;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services;
using static Utilities.Constants.RequestHandlerConstants;

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

            return result.Count switch
            {
                > 0 => this.HandleSuccess(string.Format(QueryByIdSuccessMessageFormat, tableName), result),
                _ => this.HandleNotFound(string.Format(NotFoundMessageFormat, key, tableName)),
            };
        }
        catch (Exception ex)
        {
            return ex switch
            {
                SqlException sqlEx when sqlEx.Message.Contains(SqlExceptionConversionFailed) =>
                    this.HandleBadRequest(string.Format(BadRequestMessageDataTypeFormat, key, tableName)),
                ArgumentException argEx when argEx.Message.Contains(string.Format(ArgumentExceptionNoPrimaryKeyFormat, tableName)) =>
                    this.HandleNotFound(string.Format(NotFoundMessagePrimaryKeyFormat, tableName)),
                _ => this.HandleError(string.Format(QueryByIdErrorMessageFormat, key, tableName), ex),
            };
        }
    }
}
