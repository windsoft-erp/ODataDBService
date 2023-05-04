using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Controllers.Handlers.OData.Swagger;
using ODataDBService.Models;
using System.Text.Json;


namespace ODataDBService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ODataV4Controller : ControllerBase
    {
        private readonly IODataRequestHandlerFactory _requestHandlerFactory;

        public ODataV4Controller(IODataRequestHandlerFactory requestHandlerFactory)
        {
            _requestHandlerFactory=requestHandlerFactory??throw new ArgumentNullException(nameof(requestHandlerFactory));
        }

        /// <summary>
        /// Queries records in a given table using OData v4 compliant query options.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /ODataV4/Customers?c$select=FirstName,LastName&amp;$filter=City eq 'Seattle'&amp;$orderby=LastName&amp;$top=10&amp;$skip=0
        ///
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
            "$skip=0"
        )]
        public Task<IActionResult> QueryAsync(
            string tableName,
            [FromQuery(Name = "$select")] string? select = null,
            [FromQuery(Name = "$filter")] string? filter = null,
            [FromQuery(Name = "$apply")] string? apply = null,
            [FromQuery(Name = "$orderby")] string? orderby = null,
            [FromQuery(Name = "$top")] int top = 10,
            [FromQuery(Name = "$skip")] int skip = 0) =>
            _requestHandlerFactory.CreateQueryHandler().HandleAsync(tableName, HttpContext.Request.Query);
        
        /// <summary>
        /// Gets a record from the specified table by key.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /ODataV4/Customers('ALFKI')
        ///
        /// </remarks>
        /// <param name="tableName">The name of the table containing the record to get.</param>
        /// <param name="key">The key of the record to get.</param>
        /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
        [HttpGet("{tableName}({key})")]
        public Task<IActionResult> QueryByIdAsync(string tableName, string key) =>
            _requestHandlerFactory.CreateQueryByIdRequestHandler().HandleAsync(tableName, key);

        /// <summary>
        /// Deletes a record from the specified table by key.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /ODataV4/Customers('ALFKI')
        ///
        /// </remarks>
        /// <param name="tableName">The name of the table containing the record to delete.</param>
        /// <param name="key">The key of the record to delete.</param>
        /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
        [HttpDelete("{tableName}({key})")]
        public Task<IActionResult> DeleteAsync(string tableName, string key) =>
            _requestHandlerFactory.CreateDeleteHandler().HandleAsync(tableName, key);

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
        /// </remarks>
        /// <param name="tableName">The name of the table to insert the record into.</param>
        /// <param name="data">The JSON data for the new record.</param>
        /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
        [HttpPost("{tableName}")]
        public Task<IActionResult> PostAsync(string tableName, [FromBody] JsonElement data) =>
            _requestHandlerFactory.CreateInsertHandler().HandleAsync(tableName, data);

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
        /// </remarks>
        /// <param name="tableName">The name of the table containing the record to update.</param>
        /// <param name="key">The key of the record to update.</param>
        /// <param name="data">The JSON data for the updated record.</param>
        /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
        [HttpPut("{tableName}({key})")]
        public Task<IActionResult> PutAsync(string tableName, string key, [FromBody] JsonElement data) =>
            _requestHandlerFactory.CreateUpdateHandler().HandleAsync(tableName, key, data);

        /// <summary>
        /// Invalidates the table info (information about PK and columns) cache for the specified table.
        /// </summary>
        /// <remarks>
        /// This method is used to clear the cache for the specified table, forcing a reload of the table schema and metadata
        /// the next time a request is made.
        /// 
        /// Sample request:
        ///
        ///     DELETE /ODataV4/invalidate-cache/Customers
        ///
        /// </remarks>
        /// <param name="tableName">The name of the table to invalidate the cache for.</param>
        /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
        [HttpDelete("invalidate-cache/{tableName}")]
        public IActionResult InvalidateTableInfoCache(string tableName) =>
            _requestHandlerFactory.CreateInvalidateCacheHandler().Handle(tableName);

        /// <summary>
        /// Sends a batch of requests to the server.
        /// </summary>
        /// <remarks>
        /// This method is used to send a batch of OData requests to the server, allowing multiple requests to be processed
        /// in a single HTTP request. The requests are specified in the body of the HTTP request in the format of an OData
        /// batch request.
        /// </remarks>
        /// <returns>
        /// An IActionResult indicating the success or failure of the operation. The result will contain a list of
        /// <see cref="BatchResponseContent"/> objects, each of which represents the response for a single request in the batch.
        /// </returns>
        /// <example>
        /// This example demonstrates how to send a batch request to the server.
        /// 
        /// POST /$batch
        /// Content-Type: multipart/mixed;boundary=batch_boundary
        /// 
        /// --batch_boundary
        /// Content-Type: application/http
        /// Content-Transfer-Encoding:binary
        /// 
        /// POST /api/orders HTTP/1.1
        /// Content-Type: application/json
        /// 
        /// {"orderId":1,"customerId":1,"orderDate":"2023-04-29T00:00:00Z","totalAmount":123.45}
        /// 
        /// --batch_boundary
        /// Content-Type: application/http
        /// Content-Transfer-Encoding:binary
        /// 
        /// GET /api/customers?$filter=name eq 'John'
        /// 
        /// --batch_boundary--
        /// </example>
        [HttpPost("$batch")]
        public Task<IActionResult> BatchAsync() => 
            _requestHandlerFactory.CreateBatchRequestHandler().ProcessBatchRequestAsync(Request);
    }
}