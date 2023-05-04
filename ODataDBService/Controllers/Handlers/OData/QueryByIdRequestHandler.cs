using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Services;

namespace ODataDBService.Controllers.Handlers.OData;
public class QueryByIdRequestHandler : BaseRequestHandler, IQueryByIdRequestHandler
{
    private readonly IODataV4Service _oDataV4Service;

    public QueryByIdRequestHandler(ILogger<QueryByIdRequestHandler> logger, IODataV4Service oDataV4Service) : base(logger)
    {
        _oDataV4Service=oDataV4Service??throw new ArgumentNullException(nameof(oDataV4Service));
    }

    public async Task<IActionResult> HandleAsync(string tableName, string key)
    {
        try
        {
            var result = await _oDataV4Service.QueryByIdAsync(tableName, key);

            return result switch
            {
                not null => HandleCreated($"Successfully got record from table '{tableName}'.", result),
                _ => HandleNotFound($"Error getting record from table '{tableName}'."),
            };

        }
        catch (Exception ex)
        {
            var errorMessage = $"Error getting record from table '{tableName}'.";
            return HandleError(errorMessage, ex);
        }
    }
}
