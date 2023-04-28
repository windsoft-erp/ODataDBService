using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Controllers.Handlers;
using ODataDBService.Services;
using System.Text.Json;

namespace ODataDBService.Controllers.Handlers.OData
{
    public class InsertRequestHandler : BaseRequestHandler, IInsertRequestHandler
    {
        private readonly IODataV4Service _oDataV4Service;

        public InsertRequestHandler(ILogger<InsertRequestHandler> logger, IODataV4Service oDataV4Service) : base(logger)
        {
            _oDataV4Service=oDataV4Service??throw new ArgumentNullException(nameof(oDataV4Service));
        }

        public async Task<IActionResult> HandleAsync(string tableName, JsonElement data)
        {
            try
            {
                var result = await _oDataV4Service.InsertAsync(tableName, data);

                return result
                    ? HandleSuccess($"Successfully inserted record into table '{tableName}'")
                    : HandleNotFound($"Error inserting record into table '{tableName}'");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error inserting record into table '{tableName}'";
                return HandleError(errorMessage, ex);
            }
        }
    }
}