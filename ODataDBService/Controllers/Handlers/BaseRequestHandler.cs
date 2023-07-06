// <copyright file="BaseRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents a base class for request handlers.
/// </summary>
/// <remarks>
/// Subclasses of this type should be documented.
/// </remarks>
public abstract class BaseRequestHandler
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected BaseRequestHandler(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Handles a successful request.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="result">The result object.</param>
    /// <param name="args">Additional arguments for the log message.</param>
    /// <returns>The action result.</returns>
    protected IActionResult HandleSuccess(string message, object? result = null, params object[] args)
    {
        this.logger.LogInformation(message, args);
        return result != null
            ? new OkObjectResult(result)
            : new OkResult();
    }

    /// <summary>
    /// Handles a not found request.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Additional arguments for the log message.</param>
    /// <returns>The action result.</returns>
    protected IActionResult HandleNotFound(string message, params object[] args)
    {
        this.logger.LogWarning(message, args);
        return new NotFoundObjectResult(string.Format(message, args));
    }

    /// <summary>
    /// Handles an error request.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">The exception.</param>
    /// <param name="args">Additional arguments for the log message.</param>
    /// <returns>The action result.</returns>
    protected IActionResult HandleError(string message, Exception ex, params object[] args)
    {
        this.logger.LogError(ex, message, args);

        // For debugging tests in linux, PLEASE delete the console.writelines
        Console.WriteLine(message);
        Console.WriteLine(ex);
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Handles a bad request.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Additional arguments for the log message.</param>
    /// <returns>The action result.</returns>
    protected IActionResult HandleBadRequest(string message, params object[] args)
    {
        this.logger.LogWarning(message, args);
        return new BadRequestObjectResult(string.Format(message, args));
    }

    /// <summary>
    /// Handles an unauthorized request.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Additional arguments for the log message.</param>
    /// <returns>The action result.</returns>
    protected IActionResult HandleUnauthorized(string message, params object[] args)
    {
        this.logger.LogWarning(message, args);
        return new UnauthorizedResult();
    }

    /// <summary>
    /// Handles a forbidden request.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Additional arguments for the log message.</param>
    /// <returns>The action result.</returns>
    protected IActionResult HandleForbidden(string message, params object[] args)
    {
        this.logger.LogWarning(message, args);
        return new ForbidResult();
    }

    /// <summary>
    /// Handles a successful response with no content.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">The log message arguments.</param>
    /// <returns>The <see cref="NoContentResult"/> object.</returns>
    protected IActionResult HandleNoContent(string message, params object[] args)
    {
        this.logger.LogInformation(message, args);
        return new NoContentResult();
    }

    /// <summary>
    /// Handles a response for a new resource that has been created.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="value">The value of the created resource.</param>
    /// <param name="uri">The URI of the created resource.</param>
    /// <param name="args">The log message arguments.</param>
    /// <returns>The <see cref="CreatedResult"/> object.</returns>
    protected IActionResult HandleCreated(string message, object value, string uri = "", params object[] args)
    {
        this.logger.LogInformation(message, args);
        return new CreatedResult(uri, value);
    }
}