using Microsoft.AspNetCore.Mvc;
using ODataDBService.Controllers.Handlers.OData;
using ODataDBService.Services;
using ODataDBService.Services.Repositories;
using System.Text.Json;

namespace ODataDBService.Controllers.Handlers.SQLCommand
{
    public class StoredProcedureRequestHandler : BaseRequestHandler, IStoredProcedureRequestHandler
    {
        private readonly ISQLCommandService _commandService;

        public StoredProcedureRequestHandler(ILogger<InvalidateCacheRequestHandler> logger, ISQLCommandService commandService) : base(logger)
        {
            _commandService=commandService??throw new ArgumentNullException(nameof(commandService));
        }

        public async Task<IActionResult> HandleAsync(string storedProcedureName, JsonElement procedureParameters)
        {
            try
            {
                var result = await _commandService.ExecuteStoredProcedureAsync<IEnumerable<dynamic>>(storedProcedureName, ConvertJsonToDictionary(procedureParameters));

                if (!result.Any())
                {
                    return HandleNotFound($"Did not retrieve any results from stored procedure '{storedProcedureName}'");
                }

                return HandleSuccess($"Successfully ran stored procude '{storedProcedureName}'");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error running stored procedure '{storedProcedureName}'";
                return HandleError(errorMessage, ex);
            }

            return null;
        }

        private static Dictionary<string, object> ConvertJsonToDictionary(JsonElement jsonElement)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var property in jsonElement.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        dictionary[property.Name]=property.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        dictionary[property.Name]=property.Value.GetDecimal();
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        dictionary[property.Name]=property.Value.GetBoolean();
                        break;
                    case JsonValueKind.Null:
                        dictionary[property.Name]=null;
                        break;
                    default:
                        throw new ArgumentException($"Unsupported JSON value kind: {property.Value.ValueKind}");
                }
            }

            return dictionary;
        }

    }
}
