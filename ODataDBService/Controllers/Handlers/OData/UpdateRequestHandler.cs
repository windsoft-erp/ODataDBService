// <copyright file="UpdateRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData;
using System.Text.Json;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services;

/// <summary>
/// Represents a class that handles HTTP PATCH requests to update existing records in the database.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IUpdateRequestHandler"/> interface and is used by the OData controller to handle PATCH requests.
/// </remarks>
/// <seealso cref="BaseRequestHandler" />
/// <seealso cref="IUpdateRequestHandler" />
public class UpdateRequestHandler : BaseRequestHandler, IUpdateRequestHandler
{
    private readonly IODataV4Service oDataV4Service;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger used to log errors and information.</param>
    /// <param name="oDataV4Service">The service used to interact with the database.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="oDataV4Service"/> is null.
    /// </exception>
    public UpdateRequestHandler(ILogger<UpdateRequestHandler> logger, IODataV4Service oDataV4Service)
        : base(logger)
    {
        this.oDataV4Service = oDataV4Service ?? throw new ArgumentNullException(nameof(oDataV4Service));
    }

    /// <summary>
    /// Handles an HTTP PATCH request to update an existing record in the database.
    /// </summary>
    /// <param name="tableName">The name of the table containing the record to update.</param>
    /// <param name="id">The ID of the record to update.</param>
    /// <param name="data">The JSON data representing the updated record.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response to the request.</returns>
    /// <remarks>
    /// This method handles PATCH requests to update an existing record in the database. The request should contain a JSON payload
    /// representing the updated record. The ID of the record to update is passed in as a parameter. The method returns an
    /// <see cref="IActionResult"/> indicating the success or failure of the update operation.
    /// </remarks>
    public async Task<IActionResult> HandleAsync(string tableName, string id, JsonElement data)
    {
        try
        {
            var result = await this.oDataV4Service.UpdateAsync(tableName, id, data);

            return result
                ? this.HandleSuccess($"Successfully updated record with ID '{id}' in table '{tableName}'.")
                : this.HandleNotFound($"Could not find record with ID '{id}' in table '{tableName}'.");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error updating record with ID '{id}' in table '{tableName}'.";
            return this.HandleError(errorMessage, ex);
        }
    }
}