using Flurl;
using Microsoft.AspNetCore.Mvc;
using ODataDBService.Models;
using ODataDBService.Services;
using System.Text.Json;

namespace ODataDBService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ODataV4Controller : ControllerBase
    {
        private readonly ILogger<ODataV4Controller> _logger;
        private readonly IODataV4Service _oDataV4Service;
        private readonly RequestHandler _requestHandler;

        public ODataV4Controller(ILogger<ODataV4Controller> logger, IODataV4Service oDataV4Service)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _oDataV4Service = oDataV4Service ?? throw new ArgumentNullException(nameof(oDataV4Service));
            _requestHandler = new RequestHandler(logger);
        }

        [HttpGet("{tableName}", Name = "QueryRecords")]
        public Task<IActionResult> QueryAsync(
            string tableName,
            [FromQuery(Name = "$select")] string? select = null,
            [FromQuery(Name = "$filter")] string? filter = null,
            [FromQuery(Name = "apply")] string? apply = null,
            [FromQuery(Name = "$orderby")] string? orderby = null,
            [FromQuery(Name = "$top")] int top = 10,
            [FromQuery(Name = "$skip")] int skip = 0)
        {

            ODataQuery oDataQuery = new ODataQuery
            {
                TableName=tableName,
                Select=select??string.Empty,
                Filter=filter??string.Empty,
                Apply=apply??string.Empty,
                OrderBy=orderby??string.Empty,
                Top=top,
                Skip=skip
            };

            return _requestHandler.HandleAsync(
                () => _oDataV4Service.QueryAsync(oDataQuery),
                result =>
                {
                    var isLastPage = result.Count <= top;
                    result.NextLink = isLastPage ? null : Url.Link("QueryRecords", new { tableName })
                        .SetQueryParam("$select", select)
                        .SetQueryParam("$filter", filter)
                        .SetQueryParam("$apply", apply)
                        .SetQueryParam("$orderby", orderby)
                        .SetQueryParam("$top", top)
                        .SetQueryParam("$skip", skip + top);

                    return Ok(result);
                },
                "Query records");
        }

        [HttpDelete("{tableName}({key})")]
        public Task<IActionResult> DeleteAsync(string tableName, string key)
        {
            return _requestHandler.HandleAsync(
                () => _oDataV4Service.DeleteAsync(tableName, key),
                result => Ok(result),
                "Delete record");
        }

        [HttpPost("{tableName}")]
        public Task<IActionResult> PostAsync(string tableName, [FromBody] JsonElement data)
        {
            return _requestHandler.HandleAsync(
                () => _oDataV4Service.InsertAsync(tableName, data),
                result => Ok(result),
                "Insert record");
        }

        [HttpPut("{tableName}({key})")]
        public Task<IActionResult> PutAsync(string tableName, string key, [FromBody] JsonElement data)
        {
            return _requestHandler.HandleAsync(
                () => _oDataV4Service.UpdateAsync(tableName, key, data),
                result => Ok(result),
                "Update record");
        }

        [HttpPost("invalidate-cache/{tableName}")]
        public IActionResult InvalidateTableInfoCache(string tableName)
        {
            _oDataV4Service.InvalidateTableInfoCache(tableName);
            return Ok(new { message = $"Table info cache invalidated for table '{tableName}'." });
        }
    }
}