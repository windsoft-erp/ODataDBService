// <copyright file="DeleteRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services;

/// <summary>
/// Handles delete requests in the OData service.
/// </summary>
public class DeleteRequestHandler : BaseRequestHandler, IDeleteRequestHandler
{
    private readonly IODataV4Service oDataV4Service;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    /// <param name="oDataV4Service">The OData V4 service to perform delete operations.</param>
    public DeleteRequestHandler(ILogger<DeleteRequestHandler> logger, IODataV4Service oDataV4Service)
        : base(logger)
    {
        this.oDataV4Service = oDataV4Service ?? throw new ArgumentNullException(nameof(oDataV4Service));
    }

    /// <summary>
    /// Handles the delete request asynchronously.
    /// </summary>
    /// <param name="tableName">The table name where the record should be deleted.</param>
    /// <param name="id">The identifier of the record to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<IActionResult> HandleAsync(string tableName, string id)
    {
        try
        {
            var result = await this.oDataV4Service.DeleteAsync(tableName, id);

            return result
                ? this.HandleSuccess($"Successfully deleted record with ID '{id}' from table '{tableName}'.")
                : this.HandleNotFound($"Could not find record with ID '{id}' in table '{tableName}'.");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error deleting record with ID '{id}' from table '{tableName}'.";
            return this.HandleError(errorMessage, ex);
        }
    }
}