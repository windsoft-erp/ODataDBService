// <copyright file="BatchRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Controllers.Handlers.OData;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Models;

/// <summary>
/// Represents a handler for processing batch requests.
/// </summary>
public class BatchRequestHandler : BaseRequestHandler, IBatchRequestHandler
{
    private readonly IODataRequestHandlerFactory requestHandlerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="requestHandlerFactory">The factory instance to use for creating request handlers.</param>
    public BatchRequestHandler(ILogger<BatchRequestHandler> logger, IODataRequestHandlerFactory requestHandlerFactory)
        : base(logger)
    {
        this.requestHandlerFactory = requestHandlerFactory ?? throw new ArgumentNullException(nameof(requestHandlerFactory));
    }

    /// <summary>
    /// Processes the batch request asynchronously.
    /// </summary>
    /// <param name="request">The HTTP request to process.</param>
    /// <returns>An asynchronous task that represents the operation, containing the result of the batch request.</returns>
    public async Task<IActionResult> ProcessBatchRequestAsync(HttpRequest request)
    {
        try
        {
            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var contentTypeHeader) ||
                (contentTypeHeader.MediaType != null &&
                 !contentTypeHeader.MediaType.Equals("multipart/mixed", StringComparison.OrdinalIgnoreCase)))
            {
                return this.HandleBadRequest("Invalid content type for batch request. Expected 'multipart/mixed'.");
            }

            var batchBoundary = contentTypeHeader.Parameters
                .FirstOrDefault(p => p.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrEmpty(batchBoundary))
            {
                return this.HandleBadRequest(
                    "No boundary parameter found in the 'Content-Type' header of the batch request.");
            }

            var responses = new List<BatchResponseItem>();
            var reader = new MultipartReader(batchBoundary, request.Body);

            while (await reader.ReadNextSectionAsync() is MultipartSection section)
            {
                var subRequest = await this.CreateSubRequestAsync(request, section);
                var subResponse = await this.ProcessSubRequestAsync(subRequest);
                responses.Add(subResponse);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var responseContent = new MultipartContent("mixed", batchBoundary);

            foreach (var batchResponse in responses)
            {
                responseContent.Add(batchResponse.ToHttpContent());
            }

            response.Content = responseContent;

            return this.HandleSuccess("Successfully executed batch request", new BatchResult(response));
        }
        catch (Exception ex)
        {
            var errorMessage = "There was an error executing the batch request";
            return this.HandleError(errorMessage, ex);
        }
    }

    private async Task<HttpRequest> CreateSubRequestAsync(HttpRequest originalRequest, MultipartSection section)
    {
        var subRequest = new DefaultHttpContext().Request;

        using (var streamReader = new StreamReader(section.Body))
        {
            var requestLine = await streamReader.ReadLineAsync();
            if (requestLine != null)
            {
                var parts = requestLine.Split(' ');

                if (parts.Length == 3)
                {
                    subRequest.Method = parts[0];

                    // Parse the path and query string separately
                    var pathAndQuery = parts[1];
                    var pathEndIndex = pathAndQuery.IndexOf('?');
                    if (pathEndIndex >= 0)
                    {
                        subRequest.Path = pathAndQuery.Substring(0, pathEndIndex);
                        subRequest.QueryString = new QueryString(pathAndQuery.Substring(pathEndIndex));
                    }
                    else
                    {
                        subRequest.Path = pathAndQuery;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid request line in the batch request.");
                }
            }

            var content = await streamReader.ReadToEndAsync();
            var bytes = Encoding.UTF8.GetBytes(content);
            subRequest.Body = new MemoryStream(bytes);
        }

        foreach (var header in originalRequest.Headers)
        {
            subRequest.Headers[header.Key] = header.Value;
        }

        return subRequest;
    }

    private async Task<BatchResponseItem> ProcessSubRequestAsync(HttpRequest subRequest)
    {
        if (subRequest.Path.Value == null)
        {
            return new BatchResponseItem(HttpStatusCode.BadRequest, "Invalid request URL in batch operation.");
        }

        var pathSegments = subRequest.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        if (pathSegments.Length < 2)
        {
            return new BatchResponseItem(HttpStatusCode.BadRequest, "Invalid request URL in batch operation.");
        }

        var tableName = pathSegments[1];
        var match = Regex.Match(pathSegments[1], @"^(\w+)\((\d+)\)$");
        if (match.Success)
        {
            // This is a query by ID request
            tableName = match.Groups[1].Value;
        }

        IActionResult result;

        switch (subRequest.Method.ToUpperInvariant())
        {
            case "GET":
                if (match.Success)
                {
                    // This is a query by ID request
                    var id = int.Parse(match.Groups[2].Value);
                    result = await this.requestHandlerFactory.CreateQueryByIdHandler().HandleAsync(tableName, id.ToString());
                }
                else
                {
                    // This is a request for a collection
                    result = await this.requestHandlerFactory.CreateQueryHandler().HandleAsync(tableName, subRequest.Query);
                }

                break;
            case "POST":
                var postPayload = await JsonSerializer.DeserializeAsync<JsonElement>(subRequest.Body);
                result = await this.requestHandlerFactory.CreateInsertHandler().HandleAsync(tableName, postPayload);
                break;
            case "PUT":
                var key = pathSegments[2];
                var putPayload = await JsonSerializer.DeserializeAsync<JsonElement>(subRequest.Body);
                result = await this.requestHandlerFactory.CreateUpdateHandler().HandleAsync(tableName, key, putPayload);
                break;
            case "DELETE":
                var deleteKey = pathSegments[2];
                result = await this.requestHandlerFactory.CreateDeleteHandler().HandleAsync(tableName, deleteKey);
                break;
            default:
                return new BatchResponseItem(HttpStatusCode.BadRequest, $"Invalid request method '{subRequest.Method}' in batch operation.");
        }

        return result switch
        {
            OkObjectResult objectResult => new BatchResponseItem(HttpStatusCode.OK, "OK", new StringContent(JsonSerializer.Serialize(objectResult.Value))),
            NotFoundResult => new BatchResponseItem(HttpStatusCode.NotFound, "Not Found"),
            _ => new BatchResponseItem(HttpStatusCode.BadRequest, $"Invalid result type '{result.GetType().Name}' in batch operation."),
        };
    }
}