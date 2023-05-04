using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Services;
using System.Text.Json;

namespace ODataDBService.Controllers.Handlers.OData;

public class UpdateRequestHandler : BaseRequestHandler, IUpdateRequestHandler
{
    private readonly IODataV4Service _oDataV4Service;

    public UpdateRequestHandler(ILogger<UpdateRequestHandler> logger, IODataV4Service oDataV4Service) : base(logger)
    {
        _oDataV4Service=oDataV4Service??throw new ArgumentNullException(nameof(oDataV4Service));
    }

    public async Task<IActionResult> HandleAsync(string tableName, string id, JsonElement data)
    {
        try
        {
            var result = await _oDataV4Service.UpdateAsync(tableName, id, data);

            return result
                ? HandleSuccess($"Successfully updated record with ID '{id}' in table '{tableName}'.")
                : HandleNotFound($"Could not find record with ID '{id}' in table '{tableName}'.");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error updating record with ID '{id}' in table '{tableName}'.";
            return HandleError(errorMessage, ex);
        }
    }
}

