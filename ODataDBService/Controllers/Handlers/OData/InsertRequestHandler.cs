using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Services;
using System.Text.Json;

namespace ODataDBService.Controllers.Handlers.OData;
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

            return result switch
            {
                not null => HandleCreated($"Successfully inserted record into table '{tableName}'.", result),
                _ => HandleNotFound($"Error inserting record into table '{tableName}'."),
            };

        }
        catch (Exception ex)
        {
            return ex switch
            {
                SqlException sqlEx when sqlEx.Message.Contains("Violation of PRIMARY KEY constraint") => HandleBadRequest($"Error inserting the record into '{tableName}', PRIMARY KEY violation."),
                SqlException sqlEx when sqlEx.Message.Contains("Conversion failed") => HandleBadRequest($"Error inserting the record into '{tableName}', corrupted data present in request body."),
                _ => HandleError( $"Error inserting record into table '{tableName}'.", ex),
            };
        }
    }
}
