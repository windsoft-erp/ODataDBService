using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ODataDBService.Controllers.Handlers.OData
{
    public class BatchRequestHandler : BaseRequestHandler, IBatchRequestHandler
    {
        private readonly IODataRequestHandlerFactory _requestHandlerFactory;

        public BatchRequestHandler(ILogger logger, IODataRequestHandlerFactory requestHandlerFactory) : base(logger)
        {
            _requestHandlerFactory=requestHandlerFactory??throw new ArgumentNullException(nameof(requestHandlerFactory));
        }

        public async Task<IActionResult> ProcessBatchRequestAsync(HttpRequest request)
        {
            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var contentTypeHeader)||
                !contentTypeHeader.MediaType.Equals("multipart/mixed", StringComparison.OrdinalIgnoreCase))
            {
                return HandleBadRequest("Invalid content type for batch request. Expected 'multipart/mixed'.");
            }

            var batchBoundary = contentTypeHeader.Parameters.FirstOrDefault(p => p.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrEmpty(batchBoundary))
            {
                return HandleBadRequest("No boundary parameter found in the 'Content-Type' header of the batch request.");
            }

            var responses = new List<BatchResponseItem>();
            var reader = new MultipartReader(batchBoundary, request.Body);

            while (await reader.ReadNextSectionAsync() is MultipartSection section)
            {
                var subRequest = await CreateSubRequestAsync(request, section);
                var subResponse = await ProcessSubRequestAsync(subRequest);
                responses.Add(subResponse);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var responseContent = new MultipartContent("mixed", batchBoundary);

            foreach (var batchResponse in responses)
            {
                responseContent.Add(batchResponse.ToHttpContent());
            }

            response.Content=responseContent;
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

                if (methodParam is not null&&!string.IsNullOrEmpty(methodParam.Value.ToString()))
                {
                    subRequest.Method=methodParam.Value.ToString();
                }
                else
                {
                    throw new InvalidOperationException("The 'method' parameter is missing or invalid in the Content-Disposition header.");
                }

                if (uriParam is not null&&!string.IsNullOrEmpty(uriParam.Value.ToString()))
                {
                    subRequest.Path=uriParam.Value.ToString();
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
                subRequest.Body=new MemoryStream(bytes);
            }

            foreach (var header in originalRequest.Headers)
            {
                subRequest.Headers[header.Key]=header.Value;
            }

            return subRequest;
        }



        private async Task<BatchResponseItem> ProcessSubRequestAsync(HttpRequest subRequest)
        {
            var pathSegments = subRequest.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length<2)
            {
                return new BatchResponseItem(HttpStatusCode.BadRequest, "Invalid request URL in batch operation.");
            }

            var controller = pathSegments[0];
            var tableName = pathSegments[1];
            IActionResult result;

            switch (subRequest.Method.ToUpperInvariant())
            {
                case "GET":
                    result=await _requestHandlerFactory.CreateQueryHandler().HandleAsync(tableName, subRequest.Query);
                    break;
                case "POST":
                    var postPayload = await JsonSerializer.DeserializeAsync<JsonElement>(subRequest.Body);
                    result=await _requestHandlerFactory.CreateInsertHandler().HandleAsync(tableName, postPayload);
                    break;
                case "PUT":
                    var key = pathSegments[2];
                    var putPayload = await JsonSerializer.DeserializeAsync<JsonElement>(subRequest.Body);
                    result=await _requestHandlerFactory.CreateUpdateHandler().HandleAsync(tableName, key, putPayload);
                    break;
                case "DELETE":
                    var deleteKey = pathSegments[2];
                    result=await _requestHandlerFactory.CreateDeleteHandler().HandleAsync(tableName, deleteKey);
                    break;
                default:
                    return new BatchResponseItem(HttpStatusCode.BadRequest, $"Invalid request method '{subRequest.Method}' in batch operation.");
            }

            return new BatchResponseItem(result);
        }
    }
}
