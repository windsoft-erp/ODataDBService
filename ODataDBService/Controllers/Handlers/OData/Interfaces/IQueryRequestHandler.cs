using Microsoft.AspNetCore.Mvc;

namespace ODataDBService.Controllers.Handlers.OData.Interfaces
{
    public interface IQueryRequestHandler
    {
        Task<IActionResult> HandleAsync(string tableName, IQueryCollection query);
    }
}