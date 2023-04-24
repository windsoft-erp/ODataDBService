using DynamicODataToSQL;
using Microsoft.AspNetCore.Mvc;
using ODataDBService.Models;
using Dapper;
using System.Data.SqlClient;
using Flurl;

namespace ODataDBService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ODataV4Controller : ControllerBase
    {
        private readonly ILogger<ODataV4Controller> _logger;
        private readonly IODataToSqlConverter _oDataToSqlConverter;
        private readonly string _connectionString;

        public ODataV4Controller(ILogger<ODataV4Controller> logger,
            IODataToSqlConverter oDataToSqlConverter,
            IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _logger=logger??throw new ArgumentNullException(nameof(logger));
            _oDataToSqlConverter=oDataToSqlConverter??throw new ArgumentNullException(nameof(oDataToSqlConverter));
            _connectionString=configuration.GetConnectionString("Sql");
        }

        [HttpGet("{tableName}", Name = "QueryRecords")]
        public async Task<IActionResult> QueryAsync(string tableName,
            [FromQuery(Name = "$select")] string? select = null,
            [FromQuery(Name = "$filter")] string? filter = null,
            [FromQuery(Name = "$orderby")] string? orderby = null,
            [FromQuery(Name = "$top")] int top = 10,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            var query = _oDataToSqlConverter.ConvertToSQL(tableName,
                    new Dictionary<string, string>
                    {
                        { "select", select },
                        { "filter", filter },
                        { "orderby", orderby },
                        { "top", (top + 1).ToString() },
                        { "skip", skip.ToString() }
                    }
                );
            await using var conn = new SqlConnection(this._connectionString);
            IEnumerable<dynamic>? rows = (await conn.QueryAsync(query.Item1, query.Item2).ConfigureAwait(false))?.ToList();

            ODataQueryResult? result = null;
            if (rows==null)
            {
                return new JsonResult(result);
            }

            var isLastPage = rows.Count()<=top;
            result=new ODataQueryResult
            {
                Count=isLastPage ? rows.Count() : rows.Count()-1,
                Value=rows.Take(top),
                NextLink=isLastPage ? null : this.BuildNextLink(tableName, @select, filter, @orderby, top, skip)
            };

            return new JsonResult(result);
        }

        private string BuildNextLink(string tableName,
            string select,
            string filter,
            string orderby,
            int top,
            int skip
            )
        {
            var nextLink = Url.Link("QueryRecords", new { tableName });
            nextLink=nextLink
                .SetQueryParam("select", select)
                .SetQueryParam("filter", filter)
                .SetQueryParam("orderBy", orderby)
                .SetQueryParam("top", top)
                .SetQueryParam("skip", skip+top);

            return nextLink;
        }
    }
}