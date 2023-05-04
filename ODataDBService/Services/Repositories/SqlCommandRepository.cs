using Dapper;
using ODataDBService.Models;
using System.Data;
using System.Data.SqlClient;

namespace ODataDBService.Services.Repositories;

public class SqlCommandRepository : ISqlCommandRepository
{
    private readonly string _connectionString;

    public SqlCommandRepository(string connectionString)
    {
        _connectionString=connectionString;
    }

    public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object>? parameters = null)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            return await connection.QueryAsync<T>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<IEnumerable<StoredProcedureParameter>> GetStoredProcedureParametersAsync(string storedProcedureName)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var parameters = await connection.QueryAsync<StoredProcedureParameter>(
                "SELECT PARAMETER_NAME AS [Name], DATA_TYPE AS [Type] FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_NAME = @StoredProcedureName",
                new { StoredProcedureName = storedProcedureName });

            return parameters;
        }
    }
}

