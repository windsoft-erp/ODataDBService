using Dapper;
using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Controllers.Handlers.SQLCommand;
using ODataDBService.Models;
using System.Data.SqlClient;
using System.Text.Json;

namespace ODataDBService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SqlCommandController : ControllerBase
    {
        private readonly IStoredProcedureRequestHandler _spRequestHandler;

        public SqlCommandController(IStoredProcedureRequestHandler spRequestHandler)
        {
            _spRequestHandler=spRequestHandler;
        }

        /// <summary>
        /// Executes the specified stored procedure with the provided parameters.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /SQLCommand/UpdateCustomer
        ///     {
        ///         "CustomerID": "1121315355235",
        ///         "Name": John Deer,
        ///         "Address": 1234 Main Street Apt. #567 Anytown, USA 12345 
        ///     }
        ///
        /// </remarks>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="procedureParameters">The parameters for the stored procedure as a JSON object.</param>
        /// <returns>The result of the stored procedure as a JSON object.</returns>
        [HttpPost("{storedProcedureName}")]
        public Task<IActionResult> ExecuteStoredProcedureAsync(string storedProcedureName, [FromBody] JsonElement procedureParameters) =>
          _spRequestHandler.HandleAsync(storedProcedureName, procedureParameters);
        
    }
}
