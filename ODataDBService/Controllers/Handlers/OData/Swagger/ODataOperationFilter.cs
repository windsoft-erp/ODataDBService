using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ODataDBService.Controllers.Handlers.OData.Swagger
{
    public class ODataOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var odataQuery = context.MethodInfo.GetCustomAttributes(true)
                .OfType<SwaggerODataQueryAttribute>().FirstOrDefault();

            if (odataQuery!=null)
            {
                foreach (var param in operation.Parameters)
                {
                    if (odataQuery.QueryExamples.TryGetValue(param.Name, out string? example))
                    {
                        param.Description+=$" Example: {example}";
                    }
                }
            }
        }
    }
}
