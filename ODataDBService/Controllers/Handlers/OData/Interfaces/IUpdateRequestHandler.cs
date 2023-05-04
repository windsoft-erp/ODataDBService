using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ODataDBService.Controllers.Handlers.OData.Interfaces;
public interface IUpdateRequestHandler
{
    Task<IActionResult> HandleAsync(string tableName, string key, JsonElement data);
}
