// <copyright file="StoredProcedureRequestHandler.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Controllers.Handlers.SqlCommand;
using System.Data.SqlClient;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Services;

/// <summary>
/// Represents a request handler for executing stored procedures.
/// </summary>
public class StoredProcedureRequestHandler : BaseRequestHandler, IStoredProcedureRequestHandler
{
    private readonly ISqlCommandService commandService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StoredProcedureRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandService">The SQL command service.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandService"/> is null.</exception>
    public StoredProcedureRequestHandler(ILogger<StoredProcedureRequestHandler> logger, ISqlCommandService commandService)
        : base(logger)
    {
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    /// <summary>
    /// Handles the execution of a stored procedure with the given name and parameters.
    /// </summary>
    /// <param name="storedProcedureName">The name of the stored procedure.</param>
    /// <param name="procedureParameters">The parameters to pass to the stored procedure as a JSON element.</param>
    /// <returns>An action result representing the outcome of the stored procedure execution.</returns>
    public async Task<IActionResult> HandleAsync(string storedProcedureName, JsonElement procedureParameters)
    {
        try
        {
            var result = await this.commandService.ExecuteStoredProcedureAsync(storedProcedureName, ConvertJsonToDictionary(procedureParameters));

            if (!result.Any())
            {
                return this.HandleSuccess($"Did not retrieve any results from stored procedure '{storedProcedureName}'");
            }

            return this.HandleSuccess($"Successfully ran stored procude '{storedProcedureName}'", result);
        }
        catch (Exception ex)
        {
            return ex switch
            {
                SqlException sqlEx when sqlEx.Message.Contains($"Could not find stored procedure '{storedProcedureName}'")
                    => this.HandleNotFound($"Could not find stored procedure: {storedProcedureName}."),
                SqlException sqlEx when !sqlEx.Message.Contains("A network-related or instance-specific error occurred ")
                    => this.HandleBadRequest($"The stored procedure '{storedProcedureName}' has thrown an error."),
                ArgumentException argEx when argEx.Message.Contains("Cannot convert")
                    => this.HandleBadRequest("Corrupted data: " + ex.Message),
                _ => this.HandleError($"Error running stored procedure '{storedProcedureName}'.", ex),
            };
        }
    }

    private static Dictionary<string, object?> ConvertJsonToDictionary(JsonElement jsonElement)
    {
        var dictionary = new Dictionary<string, object?>();

        foreach (var property in jsonElement.EnumerateObject())
        {
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.String:
                    dictionary[property.Name] = property.Value.GetString();
                    break;
                case JsonValueKind.Number:
                    dictionary[property.Name] = property.Value.GetDecimal();
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    dictionary[property.Name] = property.Value.GetBoolean();
                    break;
                case JsonValueKind.Null:
                    dictionary[property.Name] = null;
                    break;
                default:
                    throw new ArgumentException($"Unsupported JSON value kind: {property.Value.ValueKind}");
            }
        }

        return dictionary;
    }
}
