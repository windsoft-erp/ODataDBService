using Flurl;
using Microsoft.AspNetCore.Mvc;
using ODataDBService.Models;
using ODataDBService.Services;
using System.Net.Mime;
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
            [FromQuery(Name = "$orderby")] string? orderby = null,
            [FromQuery(Name = "$top")] int top = 10,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return _requestHandler.HandleAsync(
                () => _oDataV4Service.QueryAsync(tableName, select, filter, orderby, top, skip),
                result => Ok(result),
                "Query records");
        }

        [HttpDelete("{tableName}/{key}")]
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
    }

    // Note: The ExecuteStoredProcedure method should also be refactored to use the service and repository pattern.
    // ...
    //[HttpPost("{storedProcedureName}")]
    //public IActionResult ExecuteStoredProcedure(string storedProcedureName, [FromBody] JsonElement procedureParameters)
    //{
    //    // Validate the input
    //    if (procedureParameters.ValueKind != JsonValueKind.Object)
    //    {
    //        return BadRequest("Procedure parameters must be a JSON object.");
    //    }

    //    // Get the expected parameter names and types from the stored procedure
    //    var expectedParameters = GetStoredProcedureParameters(storedProcedureName);

    //    // Validate that each expected parameter is present in the input
    //    foreach (var expectedParameter in expectedParameters)
    //    {
    //        if (!procedureParameters.TryGetProperty(expectedParameter.Name, out var parameterValue))
    //        {
    //            return BadRequest($"Procedure parameter '{expectedParameter.Name}' is missing.");
    //        }

    //        // Map the JSON value kind to its corresponding SQL data type
    //        string parameterType = "";
    //        switch (parameterValue.ValueKind)
    //        {
    //            case JsonValueKind.String:
    //                parameterType = "nvarchar";
    //                break;
    //            case JsonValueKind.Number:
    //                parameterType = "decimal";
    //                break;
    //            case JsonValueKind.True:
    //            case JsonValueKind.False:
    //                parameterType = "bit";
    //                break;
    //            case JsonValueKind.Null:
    //                parameterType = "nvarchar";
    //                break;
    //            default:
    //                return BadRequest($"Procedure parameter '{expectedParameter.Name}' has an invalid type. Expected '{expectedParameter.Type}', but got '{parameterValue.ValueKind}'.");
    //        }

    //        // Validate the parameter value's type against the expected type
    //        if (parameterType != expectedParameter.Type)
    //        {
    //            return BadRequest($"Procedure parameter '{expectedParameter.Name}' has an invalid type. Expected '{expectedParameter.Type}', but got '{parameterType}'.");
    //        }
    //    }

    //    try
    //    {
    //        // Execute the stored procedure
    //        using (var connection = new SqlConnection(_connectionString))
    //        {
    //            connection.Open();
    //            var result = connection.Query<dynamic>(storedProcedureName, (object)procedureParameters, commandType: System.Data.CommandType.StoredProcedure);
    //            return Ok(result);
    //        }
    //    }
    //    catch (SqlException ex)
    //    {
    //        return StatusCode(500, ex.Message);
    //    }
    //}

    //private IEnumerable<StoredProcedureParameter> GetStoredProcedureParameters(string storedProcedureName)
    //{
    //    // Get the expected parameter names and types from the stored procedure
    //    using (var connection = new SqlConnection(_connectionString))
    //    {
    //        connection.Open();
    //        var command = new SqlCommand($"SELECT PARAMETER_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_NAME = '{storedProcedureName}'", connection);
    //        var reader = command.ExecuteReader();
    //        while (reader.Read())
    //        {
    //            yield return new StoredProcedureParameter
    //            {
    //                Name = (string)reader["PARAMETER_NAME"],
    //                Type = (string)reader["DATA_TYPE"]
    //            };
    //        }
    //    }
    //}
}
}