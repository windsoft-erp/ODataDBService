// <copyright file="QueryRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData;

using System.Data.SqlClient;
using Flurl;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OData;
using Models;
using Services;

/// <summary>
/// Handles OData query requests for a specific table.
/// </summary>
public class QueryRequestHandler : BaseRequestHandler, IQueryRequestHandler
{
    private readonly IODataV4Service oDataV4Service;
    private readonly IUrlHelper urlHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="oDataV4Service">The OData V4 service.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="urlHelperFactory">The URL helper factory.</param>
    public QueryRequestHandler(
        ILogger<QueryRequestHandler> logger,
        IODataV4Service oDataV4Service,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory)
        : base(logger)
    {
        this.oDataV4Service = oDataV4Service ?? throw new ArgumentNullException(nameof(oDataV4Service));
        this.urlHelper = urlHelperFactory?.GetUrlHelper(new ActionContext(
            httpContextAccessor?.HttpContext
            ?? new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor())) ?? throw new ArgumentNullException(nameof(urlHelperFactory));
    }

    /// <summary>
    /// Handles a request to retrieve records from a table.
    /// </summary>
    /// <param name="tableName">The name of the table to retrieve records from.</param>
    /// <param name="query">The query parameters.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response to the request.</returns>
    public async Task<IActionResult> HandleAsync(string tableName, IQueryCollection query)
    {
        try
        {
            if (query.Keys.Except(new[] { "$select", "$filter", "$orderby", "$top", "$skip", "apply" }, StringComparer.OrdinalIgnoreCase).Any())
            {
                return this.HandleBadRequest($"Invalid parameters in query string: {string.Join(",", query.Keys)}.");
            }

            var oDataQuery = this.BuildODataQuery(tableName, query);
            var result = await this.oDataV4Service.QueryAsync(oDataQuery);

            return result switch
            {
                { Count: 0 } => this.HandleNoContent($"No records found for '{tableName}'."),
                var queryResult => this.HandleQuerySuccess(queryResult, tableName, oDataQuery),
            };
        }
        catch (Exception ex)
        {
            return ex switch
            {
                SqlException sqlEx when sqlEx.Message.Contains($"Invalid object name '{tableName}'") => this.HandleNotFound($"Table {tableName} does not exist."),
                SqlException sqlEx when sqlEx.Message.Contains("Invalid column name") => this.HandleNotFound(ex.Message),
                ODataException odataEx => this.HandleBadRequest(odataEx.Message),
                _ => this.HandleError($"Error retrieving records for '{tableName}.'", ex),
            };
        }
    }

    private IActionResult HandleQuerySuccess(ODataQueryResult queryResult, string tableName, ODataQuery oDataQuery)
    {
        var isLastPage = queryResult.Count <= oDataQuery.Top;
        queryResult.NextLink = isLastPage ? null : this.urlHelper.Link("QueryRecords", new { tableName })
            .SetQueryParam("$select", oDataQuery.Select)
            .SetQueryParam("$filter", oDataQuery.Filter)
            .SetQueryParam("$apply", oDataQuery.Apply)
            .SetQueryParam("$orderby", oDataQuery.OrderBy)
            .SetQueryParam("$top", oDataQuery.Top)
            .SetQueryParam("$skip", oDataQuery.Skip + oDataQuery.Top);
        return this.HandleSuccess($"Successfully retrieved records for '{tableName}'.", queryResult);
    }

    private ODataQuery BuildODataQuery(string tableName, IQueryCollection query)
    {
        return new ODataQuery
        {
            TableName = tableName,
            Select = query["$select"].ToString() ?? string.Empty,
            Filter = query["$filter"].ToString() ?? string.Empty,
            Apply = query["apply"].ToString() ?? string.Empty,
            OrderBy = query["$orderby"].ToString() ?? string.Empty,
            Top = int.TryParse(query["$top"].ToString(), out int topValue) ? topValue : 10,
            Skip = int.TryParse(query["$skip"].ToString(), out int skipValue) ? skipValue : 0,
        };
    }
}