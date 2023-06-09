﻿// <copyright file="SqlCommandRepository.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services.Repositories;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Models;

/// <summary>
/// A repository for executing SQL commands.
/// </summary>
public class SqlCommandRepository : ISqlCommandRepository
{
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCommandRepository"/> class.
    /// </summary>
    /// <param name="configuration">The configuration used to get the connection string.</param>
    public SqlCommandRepository(IConfiguration configuration)
    {
        this.connectionString = configuration?.GetConnectionString("Sql") ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Executes the specified stored procedure asynchronously and returns the result set.
    /// </summary>
    /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">An optional dictionary of parameters to pass to the stored procedure.</param>
    /// <returns>An <see cref="IEnumerable{dynamic}"/> containing the objects returned by the stored procedure.</returns>
    public async Task<IEnumerable<dynamic>> ExecuteStoredProcedureAsync(string storedProcedureName, Dictionary<string, object?>? parameters = null)
    {
        await using (var connection = new SqlConnection(this.connectionString))
        {
            connection.Open();
            return await connection.QueryAsync<dynamic>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
        }
    }

    /// <summary>
    /// Gets the parameters of the specified stored procedure asynchronously.
    /// </summary>
    /// <param name="storedProcedureName">The name of the stored procedure.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the parameters of the stored procedure.</returns>
    public async Task<IEnumerable<StoredProcedureParameter>> GetStoredProcedureParametersAsync(string storedProcedureName)
    {
        await using (var connection = new SqlConnection(this.connectionString))
        {
            connection.Open();
            var parameters = await connection.QueryAsync<StoredProcedureParameter>(
                "SELECT PARAMETER_NAME AS [Name], DATA_TYPE AS [Type] FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_NAME = @StoredProcedureName",
                new { StoredProcedureName = storedProcedureName });

            return parameters;
        }
    }
}