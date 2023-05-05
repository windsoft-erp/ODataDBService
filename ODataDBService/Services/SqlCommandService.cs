// <copyright file="SqlCommandService.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services;
using System.Data;
using Models;
using Repositories;

/// <summary>
/// Represents a service for executing SQL commands such as stored procedures and validating input parameters.
/// </summary>
public class SqlCommandService : ISqlCommandService
{
    private readonly ISqlCommandRepository repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCommandService"/> class.
    /// </summary>
    /// <param name="repository">The SQL command repository to use for database interactions.</param>
    public SqlCommandService(ISqlCommandRepository repository)
    {
        this.repository = repository;
    }

    /// <summary>
    /// Executes the specified stored procedure with the given parameters and returns the result as a collection of type <typeparamref name="T"/>.
    /// Validates that the provided parameters match the expected parameters of the stored procedure and throws exceptions if there are any mismatches or missing parameters.
    /// </summary>
    /// <typeparam name="T">The type of object returned by the stored procedure.</typeparam>
    /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">A dictionary of the parameters to pass to the stored procedure.</param>
    /// <returns>A collection of objects of type <typeparamref name="T"/> returned by the stored procedure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if a required parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the provided parameters do not match the expected parameters of the stored procedure.</exception>
    public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object?> parameters)
    {
        var expectedParameters = await this.repository.GetStoredProcedureParametersAsync(storedProcedureName);

        this.ValidateParameters(expectedParameters, parameters);

        var result = await this.repository.ExecuteStoredProcedureAsync<T>(storedProcedureName, parameters);

        return result;
    }

    private void ValidateParameters(IEnumerable<StoredProcedureParameter> expectedParameters, Dictionary<string, object?> parameters)
    {
        // Validate that each expected parameter is present in the input
        foreach (var expectedParameter in expectedParameters)
        {
            if (expectedParameter.Name is null)
            {
                throw new ArgumentNullException(nameof(expectedParameter.Name));
            }

            if (expectedParameter.Type is null)
            {
                throw new ArgumentNullException(nameof(expectedParameter.Type));
            }

            if (!parameters.TryGetValue(expectedParameter.Name, out var parameterValue))
            {
                throw new ArgumentException($"Procedure parameter '{expectedParameter.Name}' is missing.");
            }

            var expectedType = this.GetSqlDbType(expectedParameter.Type);
            if (parameterValue == null)
            {
                continue;
            }

            var parameterType = this.GetSqlDbType(parameterValue);

            // Validate the parameter value's type against the expected type
            if (parameterType != expectedType)
            {
                throw new ArgumentException($"Procedure parameter '{expectedParameter.Name}' has an invalid type. Expected '{expectedParameter.Type}', but got '{parameterType}'.");
            }
        }
    }

    private SqlDbType GetSqlDbType(object parameterValue)
    {
        return parameterValue switch
        {
            string _ => SqlDbType.NVarChar,
            int _ => SqlDbType.Int,
            decimal _ => SqlDbType.Decimal,
            bool _ => SqlDbType.Bit,
            null => SqlDbType.NVarChar,
            _ => throw new ArgumentException($"Invalid parameter type: {parameterValue.GetType()}"),
        };
    }

    private SqlDbType GetSqlDbType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "nvarchar" => SqlDbType.NVarChar,
            "int" => SqlDbType.Int,
            "decimal" => SqlDbType.Decimal,
            "bit" => SqlDbType.Bit,
            _ => throw new ArgumentException($"Invalid SQL type: {type}"),
        };
    }
}