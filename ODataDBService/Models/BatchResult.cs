// <copyright file="BatchResult.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Models;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents a result for handling batch requests.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IActionResult"/> interface to provide a mechanism for handling batch requests.
/// </remarks>
public class BatchResult : IActionResult
{
    private readonly HttpResponseMessage response;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchResult"/> class.
    /// </summary>
    /// <param name="response">The HttpResponseMessage containing the response to be returned.</param>
    public BatchResult(HttpResponseMessage response)
    {
        this.response = response;
    }

    /// <summary>
    /// Executes the result of an HTTP operation as an asynchronous operation.
    /// </summary>
    /// <param name="context">The context in which the result is executed. The context information includes
    /// the controller, HTTP content, request context, and route data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = (int)this.response.StatusCode;

        response.ContentType = this.response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        await this.response.Content.CopyToAsync(response.Body);
    }
}