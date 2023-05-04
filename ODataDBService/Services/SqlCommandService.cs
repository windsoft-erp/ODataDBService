using ODataDBService.Models;
using ODataDBService.Services.Repositories;
using System.Data;

namespace ODataDBService.Services
{
    public class SqlCommandService : ISqlCommandService
    {
        private readonly ILogger _logger;
        private readonly ISQLCommandRepository _repository;

        public SqlCommandService(ILogger<SqlCommandService> logger, ISQLCommandRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, Dictionary<string, object> parameters)
        {
            var expectedParameters = await _repository.GetStoredProcedureParametersAsync(storedProcedureName);

            ValidateParameters(expectedParameters, parameters);

            var result = await _repository.ExecuteStoredProcedureAsync<T>(storedProcedureName, parameters);

            return result;
        }

        private void ValidateParameters(IEnumerable<StoredProcedureParameter> expectedParameters, Dictionary<string, object> parameters)
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

                var expectedType = GetSqlDbType(expectedParameter.Type);
                var parameterType = GetSqlDbType(parameterValue);

                // Validate the parameter value's type against the expected type
                if (parameterType!=expectedType)
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
}
