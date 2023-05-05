// <copyright file="BatchRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
    public BatchRequestHandler(ILogger logger, IODataRequestHandlerFactory requestHandlerFactory)
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
        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var contentTypeHeader) ||
           (contentTypeHeader.MediaType != null && contentTypeHeader.MediaType.Equals("multipart/mixed", StringComparison.OrdinalIgnoreCase)))
        {
            return this.HandleBadRequest("Invalid content type for batch request. Expected 'multipart/mixed'.");
        }

        var batchBoundary = contentTypeHeader.Parameters.FirstOrDefault(p => p.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))?.Value;
        if (string.IsNullOrEmpty(batchBoundary))
        {
            return this.HandleBadRequest("No boundary parameter found in the 'Content-Type' header of the batch request.");
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
        return new BatchResult(response);
    }

    private async Task<HttpRequest> CreateSubRequestAsync(HttpRequest originalRequest, MultipartSection section)
    {
        var subRequest = new DefaultHttpContext().Request;
        var contentDispositionHeader = section.GetContentDispositionHeader();

        if (contentDispositionHeader is not null)
        {
            var methodParam = contentDispositionHeader.Parameters.FirstOrDefault(p => p.Name.Equals("method", StringComparison.OrdinalIgnoreCase));
            var uriParam = contentDispositionHeader.Parameters.FirstOrDefault(p => p.Name.Equals("uri", StringComparison.OrdinalIgnoreCase));

            if (methodParam is not null && !string.IsNullOrEmpty(methodParam.Value.ToString()))
            {
                subRequest.Method = methodParam.Value.ToString();
            }
            else
            {
                throw new InvalidOperationException("The 'method' parameter is missing or invalid in the Content-Disposition header.");
            }

            if (uriParam is not null && !string.IsNullOrEmpty(uriParam.Value.ToString()))
            {
                subRequest.Path = uriParam.Value.ToString();
            }
            else
            {
                throw new InvalidOperationException("The 'uri' parameter is missing or invalid in the Content-Disposition header.");
            }
        }
        else
        {
            throw new InvalidOperationException("The Content-Disposition header is missing in the batch request.");
        }

        using (var streamReader = new StreamReader(section.Body))
        {
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
        IActionResult result;

        switch (subRequest.Method.ToUpperInvariant())
        {
            case "GET":
                result = await this.requestHandlerFactory.CreateQueryHandler().HandleAsync(tableName, subRequest.Query);
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

        return new BatchResponseItem(result);
    }
}