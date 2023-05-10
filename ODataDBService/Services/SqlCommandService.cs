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
    private static readonly Dictionary<string, (SqlDbType DbType, Type ClrType)> TypeMap = new()
    {
        { "bigint", (SqlDbType.BigInt, typeof(long)) },
        { "binary", (SqlDbType.Binary, typeof(byte[])) },
        { "bit", (SqlDbType.Bit, typeof(bool)) },
        { "char", (SqlDbType.Char, typeof(string)) },
        { "date", (SqlDbType.Date, typeof(DateTime)) },
        { "datetime", (SqlDbType.DateTime, typeof(DateTime)) },
        { "datetime2", (SqlDbType.DateTime2, typeof(DateTime)) },
        { "datetimeoffset", (SqlDbType.DateTimeOffset, typeof(DateTimeOffset)) },
        { "decimal", (SqlDbType.Decimal, typeof(decimal)) },
        { "float", (SqlDbType.Float, typeof(double)) },
        { "image", (SqlDbType.Image, typeof(byte[])) },
        { "int", (SqlDbType.Int, typeof(int)) },
        { "money", (SqlDbType.Money, typeof(decimal)) },
        { "nchar", (SqlDbType.NChar, typeof(string)) },
        { "ntext", (SqlDbType.NText, typeof(string)) },
        { "numeric", (SqlDbType.Decimal, typeof(decimal)) },
        { "nvarchar", (SqlDbType.NVarChar, typeof(string)) },
        { "real", (SqlDbType.Real, typeof(float)) },
        { "smalldatetime", (SqlDbType.SmallDateTime, typeof(DateTime)) },
        { "smallint", (SqlDbType.SmallInt, typeof(short)) },
        { "smallmoney", (SqlDbType.SmallMoney, typeof(decimal)) },
        { "text", (SqlDbType.Text, typeof(string)) },
        { "time", (SqlDbType.Time, typeof(TimeSpan)) },
        { "timestamp", (SqlDbType.Timestamp, typeof(byte[])) },
        { "tinyint", (SqlDbType.TinyInt, typeof(byte)) },
        { "uniqueidentifier", (SqlDbType.UniqueIdentifier, typeof(Guid)) },
        { "varbinary", (SqlDbType.VarBinary, typeof(byte[])) },
        { "varchar", (SqlDbType.VarChar, typeof(string)) },
        { "xml", (SqlDbType.Xml, typeof(string)) },
    };

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
    /// Validates that the provided parameters match the expected parameters of the stored procedure and throws exceptions if there are any mismatches or missing parameters.
    /// </summary>
    /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">A dictionary of the parameters to pass to the stored procedure.</param>
    /// <returns>A collection of objects of dynamic type returned by the stored procedure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if a required parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the provided parameters do not match the expected parameters of the stored procedure.</exception>
    public async Task<IEnumerable<dynamic>> ExecuteStoredProcedureAsync(string storedProcedureName, Dictionary<string, object?> parameters)
    {
        var expectedParameters = await this.repository.GetStoredProcedureParametersAsync(storedProcedureName);

        this.ValidateParameters(expectedParameters, parameters);

        var result = await this.repository.ExecuteStoredProcedureAsync(storedProcedureName, parameters);

        return result;
    }

    private void ValidateParameters(IEnumerable<StoredProcedureParameter> expectedParameters, Dictionary<string, object?> parameters)
    {
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

            if (!parameters.TryGetValue(expectedParameter.Name.Replace("@", string.Empty), out var parameterValue))
            {
                throw new ArgumentException($"Procedure parameter '{expectedParameter.Name}' is missing.");
            }

            var expectedType = this.GetSqlDbType(expectedParameter.Type);
            var clrType = this.GetClrType(expectedType);

            if (parameterValue == null)
            {
                continue;
            }

            switch (parameterValue)
            {
                case int _ when clrType == typeof(int):
                    break;
                case decimal decimalValue when clrType == typeof(int):
                    parameters[expectedParameter.Name.Replace("@", string.Empty)] = (int)decimalValue;
                    break;
                case string stringValue when clrType == typeof(DateTime):
                    if (!DateTime.TryParse(stringValue, out var dateTimeValue))
                    {
                        throw new ArgumentException($"Cannot convert '{stringValue}' to DateTime for procedure parameter '{expectedParameter.Name}'.");
                    }

                    parameters[expectedParameter.Name.Replace("@", string.Empty)] = dateTimeValue;
                    break;
                case var _ when parameterValue.GetType() == clrType:
                    break;
                default:
                    throw new ArgumentException($"Cannot convert '{parameterValue.GetType()}' to '{clrType}' for procedure parameter '{expectedParameter.Name}'.");
            }
        }
    }

    private SqlDbType GetSqlDbType(string type)
    {
        if (TypeMap.TryGetValue(type.ToLowerInvariant(), out var values))
        {
            return values.DbType;
        }

        throw new ArgumentException($"Invalid SQL type: {type}");
    }

    private Type GetClrType(SqlDbType dbType)
    {
        if (TypeMap.ContainsValue((dbType, typeof(object))))
        {
            throw new NotSupportedException("Table-valued parameters are not supported.");
        }

        return TypeMap.Values.FirstOrDefault(v => v.DbType == dbType).ClrType;
    }
}