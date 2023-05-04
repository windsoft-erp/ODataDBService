using Microsoft.AspNetCore.Mvc;

namespace ODataDBService.Controllers.Handlers.OData.Interfaces;
public interface IQueryByIdRequestHandler
{
    Task<IActionResult> HandleAsync(string tableName, string key);
}
