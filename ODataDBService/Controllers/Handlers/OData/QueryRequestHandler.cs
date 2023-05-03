using Flurl;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Models;
using ODataDBService.Services;
using System.Data.SqlClient;

namespace ODataDBService.Controllers.Handlers.OData
{
    public class QueryRequestHandler : BaseRequestHandler, IQueryRequestHandler
    {
        private readonly IODataV4Service _oDataV4Service;
        private readonly IUrlHelper _urlHelper;

        public QueryRequestHandler(ILogger<QueryRequestHandler> logger, IODataV4Service oDataV4Service, IHttpContextAccessor httpContextAccessor, IUrlHelperFactory urlHelperFactory) : base(logger)
        {
            _oDataV4Service=oDataV4Service??throw new ArgumentNullException(nameof(oDataV4Service));
            _urlHelper=urlHelperFactory?.GetUrlHelper(new ActionContext(httpContextAccessor?.HttpContext??new DefaultHttpContext(), new RouteData(), new ActionDescriptor()))??throw new ArgumentNullException(nameof(urlHelperFactory));
        }

        public async Task<IActionResult> HandleAsync(string tableName, IQueryCollection query)
        {
            try
            {
                if (query.Keys.Except(new[] { "$select", "$filter", "$orderby", "$top", "$skip", "apply" }, StringComparer.OrdinalIgnoreCase).Any())
                {
                    return HandleBadRequest($"Invalid parameters in query string: {string.Join(",", query.Keys)}.");
                }

                var oDataQuery = BuildODataQuery(tableName, query);
                var result = await _oDataV4Service.QueryAsync(oDataQuery);

                return result switch
                {
                    { Count: 0 } => HandleNoContent($"No records found for '{tableName}'."),
                    var queryResult => HandleQuerySuccess(queryResult, tableName, oDataQuery)
                };

            }
            catch (Exception ex)
            {
                return ex switch
                {
                    SqlException sqlEx when sqlEx.Message.Contains($"Invalid object name '{tableName}'") => HandleNotFound($"Table {tableName} does not exist."),
                    SqlException sqlEx when sqlEx.Message.Contains("Invalid column name") => HandleNotFound(ex.Message),
                    _ => HandleError($"Error retrieving records for '{tableName}.'", ex),
                };
            }
        }

        private IActionResult HandleQuerySuccess(ODataQueryResult queryResult, string tableName, ODataQuery oDataQuery)
        {
            var isLastPage = queryResult.Count<=oDataQuery.Top;
            queryResult.NextLink=isLastPage ? null : _urlHelper.Link("QueryRecords", new { tableName })
                .SetQueryParam("$select", oDataQuery.Select)
                .SetQueryParam("$filter", oDataQuery.Filter)
                .SetQueryParam("$apply", oDataQuery.Apply)
                .SetQueryParam("$orderby", oDataQuery.OrderBy)
                .SetQueryParam("$top", oDataQuery.Top)
                .SetQueryParam("$skip", oDataQuery.Skip+oDataQuery.Top);
            return HandleSuccess($"Successfully retrieved records for '{tableName}'.", queryResult);
        }

        private ODataQuery BuildODataQuery(string tableName, IQueryCollection query)
        {
            return new ODataQuery
            {
                TableName=tableName,
                Select=query["$select"].ToString()??string.Empty,
                Filter=query["$filter"].ToString()??string.Empty,
                Apply=query["apply"].ToString()??string.Empty,
                OrderBy=query["$orderby"].ToString()??string.Empty,
                Top=int.TryParse(query["$top"].ToString(), out int topValue) ? topValue : 10,
                Skip=int.TryParse(query["$skip"].ToString(), out int skipValue) ? skipValue : 0
            };
        }
    }
}
