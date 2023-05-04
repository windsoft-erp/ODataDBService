using Microsoft.AspNetCore.Mvc;

namespace ODataDBService.Controllers.Handlers;
public abstract class BaseRequestHandler
{
    private readonly ILogger _logger;

    protected BaseRequestHandler(ILogger logger)
    {
        _logger=logger;
    }

    protected IActionResult HandleSuccess(string message, object? result=null, params object[] args)
    {
        _logger.LogInformation(message, args);
        return result != null 
            ? new OkObjectResult(result)
            : new OkResult();
    }

    protected IActionResult HandleNotFound(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
        return new NotFoundObjectResult(string.Format(message, args));
    }

    protected IActionResult HandleError(string message, Exception ex, params object[] args)
    {
        _logger.LogError(ex, message, args);
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }

    protected IActionResult HandleBadRequest(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
        return new BadRequestObjectResult(string.Format(message, args));
    }

    protected IActionResult HandleUnauthorized(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
        return new UnauthorizedResult();
    }

    protected IActionResult HandleForbidden(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
        return new ForbidResult();
    }

    protected IActionResult HandleNoContent(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
        return new NoContentResult();
    }

    protected IActionResult HandleCreated(string message, object value, string uri = "", params object[] args)
    {
        _logger.LogInformation(message, args);
        return new CreatedResult(uri, value);
    }
}

