// <copyright file="SqlCommandController.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers;
using System.Text.Json;
using Handlers.SqlCommand;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The controller for handling Sql Commands.
/// </summary>
[ApiController]
[Route("[controller]")]
public class SqlCommandController : ControllerBase
{
    private readonly IStoredProcedureRequestHandler spRequestHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCommandController"/> class.
    /// </summary>
    /// <param name="spRequestHandler">The instance of the <see cref="IStoredProcedureRequestHandler"/> to use for executing stored procedures.</param>
    public SqlCommandController(IStoredProcedureRequestHandler spRequestHandler)
    {
        this.spRequestHandler = spRequestHandler;
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
    /// Sample request executes a stored procedure with parameters from the request body.
    /// </remarks>
    /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
    /// <param name="procedureParameters">The parameters for the stored procedure as a JSON object.</param>
    /// <returns>The result of the stored procedure as a JSON object.</returns>
    [HttpPost("{storedProcedureName}")]
    public Task<IActionResult> ExecuteStoredProcedureAsync(string storedProcedureName, [FromBody] JsonElement procedureParameters) =>
      this.spRequestHandler.HandleAsync(storedProcedureName, procedureParameters);
}