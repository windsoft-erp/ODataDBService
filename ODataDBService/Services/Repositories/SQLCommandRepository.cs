using Dapper;
using ODataDBService.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ODataDBService.Services.Repositories
{
    public class SQLCommandRepository : ISQLCommandRepository
    {
        private readonly string _connectionString;

        public SQLCommandRepository(string connectionString)
        {
            _connectionString=connectionString;
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object> parameters = null)
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
}
