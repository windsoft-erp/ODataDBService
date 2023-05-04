using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ODataDBService.Controllers.Handlers.OData.Interfaces;
public interface IInsertRequestHandler
{
    Task<IActionResult> HandleAsync(string tableName, JsonElement data);
}
