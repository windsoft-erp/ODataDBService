using DynamicODataToSQL;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using Flurl;
using ODataDBService.Models;
using System.Text.Json;
using System.Text;

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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _oDataToSqlConverter = oDataToSqlConverter ?? throw new ArgumentNullException(nameof(oDataToSqlConverter));
            _connectionString = configuration?.GetConnectionString("Sql") ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("{tableName}", Name = "QueryRecords")]
        public async Task<IActionResult> QueryAsync(string tableName,
            [FromQuery(Name = "$select")] string? select = null,
            [FromQuery(Name = "$filter")] string? filter = null,
            [FromQuery(Name = "$orderby")] string? orderby = null,
            [FromQuery(Name = "$top")] int top = 10,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            var (query, queryParams) = _oDataToSqlConverter.ConvertToSQL(tableName,
                    new Dictionary<string, string>
                    {
                        { "select", select },
                        { "filter", filter },
                        { "orderby", orderby },
                        { "top", (top + 1).ToString() },
                        { "skip", skip.ToString() }
                    }
                );

            await using var conn = new SqlConnection(_connectionString);
            var rows = await conn.QueryAsync<dynamic>(query, queryParams).ConfigureAwait(false);

            if (rows == null)
            {
                return NotFound();
            }

            var resultList = rows.ToList();
            var count = resultList.Count();
            var result = new ODataQueryResult
            {
                Count = count,
                Value = resultList
            };
  
            var isLastPage = count <= top;
            result.NextLink = isLastPage ? null : Url.Link("QueryRecords", new { tableName })
                .SetQueryParam("$select", select)
                .SetQueryParam("$filter", filter)
                .SetQueryParam("$orderby", orderby)
                .SetQueryParam("$top", top)
                .SetQueryParam("$skip", skip + top);

            return Ok(result);
        }

        [HttpDelete("{tableName}/{key}")]
        public async Task<IActionResult> DeleteAsync(string tableName, string key)
        {
            var (query, queryParams) = _oDataToSqlConverter.ConvertToSQLDelete(tableName, key, _connectionString);

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(query, queryParams).ConfigureAwait(false);

            if (result == 0)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("{tableName}")]
        public async Task<IActionResult> PostAsync(string tableName, [FromBody] JsonElement data)
        {
            var properties = data.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => prop.Value);

            var (query, queryParams) = _oDataToSqlConverter.ConvertToSQLInsert(tableName, properties);

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(query, queryParams).ConfigureAwait(false);

            if (result == 0)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut("{tableName}({key})")]
        public async Task<IActionResult> PutAsync(string tableName, string key, [FromBody] JsonElement data)
        {
            var properties = data.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value);

            var (query, queryParams) = _oDataToSqlConverter.ConvertToSQLUpdate(tableName, key, properties, _connectionString);

            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(query, queryParams).ConfigureAwait(false);

            if (result == 0)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("{storedProcedureName}")]
        public IActionResult ExecuteStoredProcedure(string storedProcedureName, [FromBody] JsonElement procedureParameters)
        {
            // Validate the input
            if (procedureParameters.ValueKind != JsonValueKind.Object)
            {
                return BadRequest("Procedure parameters must be a JSON object.");
            }

            // Get the expected parameter names and types from the stored procedure
            var expectedParameters = GetStoredProcedureParameters(storedProcedureName);

            // Validate that each expected parameter is present in the input
            foreach (var expectedParameter in expectedParameters)
            {
                if (!procedureParameters.TryGetProperty(expectedParameter.Name, out var parameterValue))
                {
                    return BadRequest($"Procedure parameter '{expectedParameter.Name}' is missing.");
                }

                // Map the JSON value kind to its corresponding SQL data type
                string parameterType = "";
                switch (parameterValue.ValueKind)
                {
                    case JsonValueKind.String:
                        parameterType = "nvarchar";
                        break;
                    case JsonValueKind.Number:
                        parameterType = "decimal";
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        parameterType = "bit";
                        break;
                    case JsonValueKind.Null:
                        parameterType = "nvarchar";
                        break;
                    default:
                        return BadRequest($"Procedure parameter '{expectedParameter.Name}' has an invalid type. Expected '{expectedParameter.Type}', but got '{parameterValue.ValueKind}'.");
                }

                // Validate the parameter value's type against the expected type
                if (parameterType != expectedParameter.Type)
                {
                    return BadRequest($"Procedure parameter '{expectedParameter.Name}' has an invalid type. Expected '{expectedParameter.Type}', but got '{parameterType}'.");
                }
            }

            try
            {
                // Execute the stored procedure
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var result = connection.Query<dynamic>(storedProcedureName, (object)procedureParameters, commandType: System.Data.CommandType.StoredProcedure);
                    return Ok(result);
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private IEnumerable<StoredProcedureParameter> GetStoredProcedureParameters(string storedProcedureName)
        {
            // Get the expected parameter names and types from the stored procedure
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand($"SELECT PARAMETER_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_NAME = '{storedProcedureName}'", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return new StoredProcedureParameter
                    {
                        Name = (string)reader["PARAMETER_NAME"],
                        Type = (string)reader["DATA_TYPE"]
                    };
                }
            }
        }

    }

    public static class ODataToSQLConverterExtension
    {
        public static (string, DynamicParameters) ConvertToSQLDelete(this IODataToSqlConverter oDataToSqlConverter, string tableName, string key, string connectionString)
        {
            var tableInfo = GetTableInfo(connectionString, tableName);

            var sql = $"DELETE FROM {tableInfo.TableName} WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}";
            var parameters = new DynamicParameters();
            parameters.Add(tableInfo.PrimaryKey, key);

            return (sql, parameters);
        }

        public static (string, DynamicParameters) ConvertToSQLInsert(this IODataToSqlConverter oDataToSqlConverter, string tableName, Dictionary<string, JsonElement> properties)
        {
            var columnNames = string.Join(", ", properties.Keys);
            var valueParams = string.Join(", ", properties.Keys.Select(key => $"@{key}"));

            var sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({valueParams})";
            var parameters = new DynamicParameters();

            foreach (var (key, value) in properties)
            {
                switch (value.ValueKind)
                {
                    case JsonValueKind.Null:
                        parameters.Add(key, null);
                        break;
                    case JsonValueKind.String:
                        parameters.Add(key, value.GetString());
                        break;
                    case JsonValueKind.Number:
                        if (value.TryGetInt32(out var intValue))
                        {
                            parameters.Add(key, intValue);
                        }
                        else if (value.TryGetDouble(out var doubleValue))
                        {
                            parameters.Add(key, doubleValue);
                        }
                        else if (value.TryGetDecimal(out var decimalValue))
                        {
                            parameters.Add(key, decimalValue);
                        }
                        break;
                    case JsonValueKind.True:
                        parameters.Add(key, true);
                        break;
                    case JsonValueKind.False:
                        parameters.Add(key, false);
                        break;
                    case JsonValueKind.Array:
                        parameters.Add(key, value.GetRawText());
                        break;
                    case JsonValueKind.Object:
                        parameters.Add(key, value.GetRawText());
                        break;
                }
            }

            return (sql, parameters);
        }

        public static (string, DynamicParameters) ConvertToSQLUpdate(this IODataToSqlConverter oDataToSqlConverter, string tableName, string key, Dictionary<string, JsonElement> properties, string connectionString)
        {
            var tableInfo = GetTableInfo(connectionString, tableName);

            var queryParams = new DynamicParameters();

            var sb = new StringBuilder($"UPDATE {tableInfo.TableName} SET ");
            foreach (var property in properties)
            {
                var propertyName = property.Key;
                var propertyValue = property.Value;

                if (propertyName.Equals(tableInfo.PrimaryKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (tableInfo.ColumnNames?.Contains(propertyName, StringComparer.OrdinalIgnoreCase) != true)
                {
                    throw new ArgumentException($"Column {propertyName} does not exist in table {tableName}");
                }

                sb.Append($"{propertyName} = @{propertyName}, ");
                queryParams.Add(propertyName, propertyValue.ToString());
            }

            sb.Remove(sb.Length - 2, 2); // remove last comma and space
            sb.Append($" WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}");
            queryParams.Add(tableInfo.PrimaryKey, key);

            return (sb.ToString(), queryParams);
        }

        private static TableInfo GetTableInfo(string connectionString, string tableName)
        {
            using var connection = new SqlConnection(connectionString);

            var tableInfo = new TableInfo
            {
                TableName = tableName,
                PrimaryKey = connection.QueryFirstOrDefault<string>(
                    @"SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                    WHERE TABLE_NAME = @TableName 
                    AND CONSTRAINT_NAME LIKE 'PK%'",
                    new { TableName = tableName })
            };

            if (tableInfo.PrimaryKey == null)
            {
                throw new ArgumentException($"Could not find primary key for table {tableName}");
            }

            tableInfo.ColumnNames = connection.Query<string>(
                @"SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @TableName",
                new { TableName = tableName });

            return tableInfo;
        }

        private class TableInfo
        {
            public string? TableName { get; set; }
            public string? PrimaryKey { get; set; }
            public IEnumerable<string>? ColumnNames { get; set; }
        }
    }
}