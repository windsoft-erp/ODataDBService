using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ODataDBService.Controllers.Handlers.SqlCommand;

public interface IStoredProcedureRequestHandler
{
    Task<IActionResult> HandleAsync(string storedProcedureName, JsonElement procedureParameters);
}
