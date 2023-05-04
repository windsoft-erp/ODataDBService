using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Services.Repositories;

namespace ODataDBService.Controllers.Handlers.OData;
public class InvalidateCacheRequestHandler : BaseRequestHandler, IInvalidateCacheRequestHandler
{
    private readonly IODataV4Repository _repository;

    public InvalidateCacheRequestHandler(ILogger<InvalidateCacheRequestHandler> logger, IODataV4Repository repository) : base(logger)
    {
        _repository=repository??throw new ArgumentNullException(nameof(repository));
    }

    public IActionResult Handle(string tableName)
    {
        var result = _repository.InvalidateTableInfoCache(tableName);

        return result
            ? HandleSuccess($"Table info cache for '{tableName}' has been invalidated.")
            : HandleNotFound($"Table '{tableName}' not found in the cache.");
    }
}