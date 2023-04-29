using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace ODataDBService.Models
{
    public class BatchResponseItem
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public HttpContent? Content { get; }

        public BatchResponseItem(HttpStatusCode statusCode, string reasonPhrase, HttpContent content = null)
        {
            StatusCode=statusCode;
            ReasonPhrase=reasonPhrase;
            Content=content;
        }

        public BatchResponseItem(IActionResult result)
        {
            StatusCode=result switch
            {
                ObjectResult objResult => (HttpStatusCode)objResult.StatusCode.GetValueOrDefault((int)HttpStatusCode.OK),
                StatusCodeResult statusCodeResult => (HttpStatusCode)statusCodeResult.StatusCode,
                _ => throw new ArgumentException("Unsupported IActionResult type.", nameof(result))
            };

            ReasonPhrase=StatusCode.ToString();

            if (result is ObjectResult objectResult)
            {
                var value = objectResult.Value;
                Content=new StringContent(value?.ToString()??"");
                Content.Headers.ContentType=new MediaTypeHeaderValue("application/json");
            }
        }

        public HttpContent ToHttpContent()
        {
            var content = new StringContent($"HTTP/1.1 {(int)StatusCode} {ReasonPhrase}\r\n");
            if (Content!=null)
            {
                content.Headers.ContentType=new MediaTypeHeaderValue("application/http")
                {
                    Parameters={ new NameValueHeaderValue("msgtype", "response") }
                };

                content.Headers.Add("Content-Transfer-Encoding", "binary");
                content.Headers.Add("Content-ID", Guid.NewGuid().ToString());
            }

            return content;
        }
    }
}
