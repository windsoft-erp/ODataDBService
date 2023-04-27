using Microsoft.AspNetCore.Mvc;

public class RequestHandler
{
    private readonly ILogger _logger;

    public RequestHandler(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> HandleAsync<T>(Func<Task<T>> action, Func<T, IActionResult> successResult, string actionDescription)
    {
        try
        {
            var result = await action();
            if (result == null)
            {
                _logger.LogWarning("{actionDescription}: NotFound", actionDescription);
                return new NotFoundResult();
            }

            _logger.LogInformation("{actionDescription}: Success", actionDescription);
            return successResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{actionDescription}: Error", actionDescription);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}