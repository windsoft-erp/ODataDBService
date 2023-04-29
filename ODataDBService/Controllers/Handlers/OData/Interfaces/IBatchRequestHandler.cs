using Microsoft.AspNetCore.Mvc;
using ODataDBService.Models;

namespace ODataDBService.Controllers.Handlers.OData.Interfaces
{
    public interface IBatchRequestHandler
    {
        Task<IActionResult> ProcessBatchRequestAsync(HttpRequest request);
    }
}
