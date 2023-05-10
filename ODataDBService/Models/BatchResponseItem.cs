// <copyright file="BatchResponseItem.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Models;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents an item in a batch response, which contains information about the status code, reason phrase, and content.
/// </summary>
/// <remarks>
/// The class can be constructed either from an <see cref="IActionResult"/> or directly from a status code, reason phrase, and content.
/// It can also be converted to an <see cref="HttpContent"/> object for use in a batch response message.
/// </remarks>
public class BatchResponseItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchResponseItem"/> class
    /// with the specified status code, reason phrase, content, and result object.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="reasonPhrase">The reason phrase associated with the status code.</param>
    /// <param name="content">The content of the response as an <see cref="HttpContent"/> instance.</param>
    /// <param name="result">The result object returned by the sub-request.</param>
    public BatchResponseItem(HttpStatusCode statusCode, string reasonPhrase, HttpContent? content = null, object? result = null)
    {
        this.StatusCode = statusCode;
        this.ReasonPhrase = reasonPhrase;
        this.Content = content;
        this.Result = result;
    }

    private HttpStatusCode StatusCode { get; }

    private string ReasonPhrase { get; }

    private HttpContent? Content { get; }

    private object? Result { get; }

    /// <summary>
    /// Converts the <see cref="BatchResponseItem"/> instance to an <see cref="HttpContent"/> instance.
    /// </summary>
    /// <returns>The converted <see cref="HttpContent"/> instance.</returns>
    public HttpContent ToHttpContent()
    {
        var content = new StringContent($"HTTP/1.1 {(int)this.StatusCode} {this.ReasonPhrase}\r\n");
        if (this.Content != null)
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var contentString = this.Content.ReadAsStringAsync().Result;
            contentString = contentString.Trim('\"');
            contentString = $"{{\"value\": {contentString}}}";
            content = new StringContent(contentString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        content.Headers.ContentType = new MediaTypeHeaderValue("application/http")
        {
            Parameters = { new NameValueHeaderValue("msgtype", "response") },
        };
        content.Headers.Add("Content-Transfer-Encoding", "binary");
        content.Headers.Add("Content-ID", Guid.NewGuid().ToString());

        return content;
    }
}