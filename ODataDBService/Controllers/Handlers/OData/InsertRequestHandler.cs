// <copyright file="InsertRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData;
using System.Data.SqlClient;
using System.Text.Json;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services;

/// <summary>
/// Represents the handler for processing insert requests for a given OData entity set.
/// </summary>
public class InsertRequestHandler : BaseRequestHandler, IInsertRequestHandler
{
    private readonly IODataV4Service oDataV4Service;

    /// <summary>
    /// Initializes a new instance of the <see cref="InsertRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="oDataV4Service">The OData V4 service.</param>
    /// <exception cref="System.ArgumentNullException">oDataV4Service.</exception>
    public InsertRequestHandler(ILogger<InsertRequestHandler> logger, IODataV4Service oDataV4Service)
        : base(logger)
    {
        this.oDataV4Service = oDataV4Service ?? throw new ArgumentNullException(nameof(oDataV4Service));
    }

    /// <summary>
    /// Inserts a record into a specified table.
    /// </summary>
    /// <param name="tableName">The name of the table to insert the record into.</param>
    /// <param name="data">The record to insert.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the status of the operation.</returns>
    public async Task<IActionResult> HandleAsync(string tableName, JsonElement data)
    {
        try
        {
            var result = await this.oDataV4Service.InsertAsync(tableName, data);

            return result switch
            {
                not null => this.HandleCreated($"Successfully inserted record into table '{tableName}'.", result),
                _ => this.HandleNotFound($"Error inserting record into table '{tableName}'."),
            };
        }
        catch (Exception ex)
        {
            return ex switch
            {
                SqlException sqlEx when sqlEx.Message.Contains("Violation of PRIMARY KEY constraint") => this.HandleBadRequest($"Error inserting the record into '{tableName}', PRIMARY KEY violation."),
                SqlException sqlEx when sqlEx.Message.Contains("Conversion failed") => this.HandleBadRequest($"Error inserting the record into '{tableName}', corrupted data present in request body."),
                _ => this.HandleError($"Error inserting record into table '{tableName}'.", ex),
            };
        }
    }
}
