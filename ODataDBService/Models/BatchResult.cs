using Microsoft.AspNetCore.Mvc;

namespace ODataDBService.Models;
public class BatchResult : IActionResult
{
    private readonly HttpResponseMessage _response;

    public BatchResult(HttpResponseMessage response)
    {
        _response=response;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode=(int)_response.StatusCode;

        if (_response.Content!=null)
        {
            response.ContentType=_response.Content.Headers.ContentType?.ToString()??"application/octet-stream";
            await _response.Content.CopyToAsync(response.Body);
        }
    }
}

