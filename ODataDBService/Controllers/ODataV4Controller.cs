// <copyright file="ODataV4Controller.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers;
using System.Text.Json;
using Handlers.OData.Interfaces;
using Handlers.OData.Swagger;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The controller for handling OData V4 requests.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ODataV4Controller : ControllerBase
{
    private readonly IODataRequestHandlerFactory requestHandlerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataV4Controller"/> class.
    /// </summary>
    /// <param name="requestHandlerFactory">The factory for creating OData request handlers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="requestHandlerFactory"/> is <c>null</c>.</exception>
    public ODataV4Controller(IODataRequestHandlerFactory requestHandlerFactory)
    {
        this.requestHandlerFactory = requestHandlerFactory ?? throw new ArgumentNullException(nameof(requestHandlerFactory));
    }

    /// <summary>
    /// Queries records in a given table using OData v4 compliant query options.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /ODataV4/Customers?c$select=FirstName,LastName&amp;$filter=City eq 'Seattle'&amp;$orderby=LastName&amp;$top=10&amp;$skip=0
    ///
    /// The sample request will query the Customers table with OData query parameters from the query string.
    /// </remarks>
    /// <param name="tableName">The name of the table to query.</param>
    /// <param name="select">A comma-separated list of properties to select from the table.</param>
    /// <param name="filter">An OData filter expression to filter the records.</param>
    /// <param name="apply">An OData apply expression for data aggregation and transformations.</param>
    /// <param name="orderby">An OData orderby expression to order the records.</param>
    /// <param name="top">The maximum number of records to return.</param>
    /// <param name="skip">The number of records to skip before starting to return records.</param>
    /// <returns>A list of records matching the specified query options.</returns>
    [HttpGet("{tableName}")]
    [SwaggerODataQueryAttribute(
        "$select=FirstName,LastName",
        "$filter=City eq 'Seattle'",
        "$apply=groupby((City), sum(Revenue))",
        "$orderby=LastName",
        "$top=10",
        "$skip=0")]
    public Task<IActionResult> QueryAsync(
        string tableName,
        [FromQuery(Name = "$select")] string? select = null,
        [FromQuery(Name = "$filter")] string? filter = null,
        [FromQuery(Name = "$apply")] string? apply = null,
        [FromQuery(Name = "$orderby")] string? orderby = null,
        [FromQuery(Name = "$top")] int top = 10,
        [FromQuery(Name = "$skip")] int skip = 0) =>
        this.requestHandlerFactory.CreateQueryHandler().HandleAsync(tableName, this.HttpContext.Request.Query);

    /// <summary>
    /// Gets a record from the specified table by key.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /ODataV4/Customers('ALFKI')
    ///
    /// The sample request will query the Customers table by the primary key value.
    /// </remarks>
    /// <param name="tableName">The name of the table containing the record to get.</param>
    /// <param name="key">The key of the record to get.</param>
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
    [HttpGet("{tableName}({key})")]
    public Task<IActionResult> QueryByIdAsync(string tableName, string key) =>
        this.requestHandlerFactory.CreateQueryByIdRequestHandler().HandleAsync(tableName, key);

    /// <summary>
    /// Deletes a record from the specified table by key.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /ODataV4/Customers('ALFKI')
    ///
    /// The sample request will delete the Customers record with primary key value.
    /// </remarks>
    /// <param name="tableName">The name of the table containing the record to delete.</param>
    /// <param name="key">The key of the record to delete.</param>
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
    [HttpDelete("{tableName}({key})")]
    public Task<IActionResult> DeleteAsync(string tableName, string key) =>
        this.requestHandlerFactory.CreateDeleteHandler().HandleAsync(tableName, key);

    /// <summary>
    /// Inserts a new record into the specified table.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /ODataV4/Customers
    ///     {
    ///         "FirstName": "John",
    ///         "LastName": "Doe",
    ///         "City": "Seattle",
    ///         "Country": "USA"
    ///     }
    ///
    /// The sample request will insert the Customers record with data present in request body.
    /// </remarks>
    /// <param name="tableName">The name of the table to insert the record into.</param>
    /// <param name="data">The JSON data for the new record.</param>
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
    [HttpPost("{tableName}")]
    public Task<IActionResult> PostAsync(string tableName, [FromBody] JsonElement data) =>
        this.requestHandlerFactory.CreateInsertHandler().HandleAsync(tableName, data);

    /// <summary>
    /// Updates a record in the specified table by key.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /ODataV4/Customers('ALFKI')
    ///     {
    ///         "FirstName": "John",
    ///         "LastName": "Doe",
    ///         "City": "Seattle",
    ///         "Country": "USA"
    ///     }
    ///
    /// The sample request will update the Customers record by primary key with data present in request body.
    /// </remarks>
    /// <param name="tableName">The name of the table containing the record to update.</param>
    /// <param name="key">The key of the record to update.</param>
    /// <param name="data">The JSON data for the updated record.</param>
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
    [HttpPut("{tableName}({key})")]
    public Task<IActionResult> PutAsync(string tableName, string key, [FromBody] JsonElement data) =>
        this.requestHandlerFactory.CreateUpdateHandler().HandleAsync(tableName, key, data);

    /// <summary>
    /// Invalidates the table info (information about PK and columns) cache for the specified table.
    /// </summary>
    /// <remarks>
    /// This method is used to clear the cache for the specified table, forcing a reload of the table schema and metadata
    /// the next time a request is made.
    /// Sample request:
    ///
    ///     DELETE /ODataV4/invalidate-cache/Customers
    ///
    /// The sample requests deletes cache for the table.
    /// </remarks>
    /// <param name="tableName">The name of the table to invalidate the cache for.</param>
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
    [HttpDelete("invalidate-cache/{tableName}")]
    public IActionResult InvalidateTableInfoCache(string tableName) =>
        this.requestHandlerFactory.CreateInvalidateCacheHandler().Handle(tableName);

    /// <summary>
    /// Sends a batch of OData requests to the server.
    /// </summary>
    /// <remarks>
    /// Use this endpoint to send a batch of OData requests to the server. Multiple requests can be included in the body
    /// of the HTTP request using the format of an OData batch request. The server processes each request and returns the
    /// response for each request in the batch.
    ///
    /// Sample request:
    ///
    ///     POST /ODataV4/$batch
    ///     Content-Type: multipart/mixed; boundary=batch_boundary
    ///
    ///     --batch_boundary
    ///     Content-Type: application/http
    ///     Content-Transfer-Encoding: binary
    ///
    ///     POST /ODataV4/Customers HTTP/1.1
    ///     Content-Type: application/json
    ///
    ///     {"id":1,"name":"John Smith"}
    ///
    ///     --batch_boundary
    ///     Content-Type: application/http
    ///     Content-Transfer-Encoding: binary
    ///
    ///     GET /ODataV4/Orders?$filter=customer_id eq 1
    ///
    ///     --batch_boundary--
    ///
    /// Sample request will execute a batch with the specified ODataV4 commands.
    /// </remarks>
    /// <returns>
    /// An IActionResult indicating the success or failure of the operation. The result will contain a list of
    /// objects each of which will contain the appropriate response for that specific batch operation.
    /// </returns>
    [HttpPost("$batch")]
    public Task<IActionResult> BatchAsync() =>
        this.requestHandlerFactory.CreateBatchRequestHandler().ProcessBatchRequestAsync(this.Request);
}
