using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Services;

namespace ODataDBService.Controllers.Handlers.OData;
public class DeleteRequestHandler : BaseRequestHandler, IDeleteRequestHandler
{
    private readonly IODataV4Service _oDataV4Service;

    public DeleteRequestHandler(ILogger<DeleteRequestHandler> logger, IODataV4Service oDataV4Service) : base(logger)
    {
        _oDataV4Service=oDataV4Service??throw new ArgumentNullException(nameof(oDataV4Service));
    }

    public async Task<IActionResult> HandleAsync(string tableName, string id)
    {
        try
        {
            var result = await _oDataV4Service.DeleteAsync(tableName, id);

            return result
                ? HandleSuccess($"Successfully deleted record with ID '{id}' from table '{tableName}'.")
                : HandleNotFound($"Could not find record with ID '{id}' in table '{tableName}'.");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error deleting record with ID '{id}' from table '{tableName}'.";
            return HandleError(errorMessage, ex);
        }
    }
}

