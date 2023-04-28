using Microsoft.AspNetCore.Mvc;

namespace ODataDBService.Controllers.Handlers.OData.Interfaces
{
    public interface IInvalidateCacheRequestHandler
    {
        IActionResult Handle(string tableName);
    }
}