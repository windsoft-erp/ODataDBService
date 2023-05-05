// <copyright file="ODataOperationFilter.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData.Swagger;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Implements the Swagger operation filter for OData requests.
/// </summary>
public class ODataOperationFilter : IOperationFilter
{
    /// <summary>
    /// Applies the filter to the specified operation.
    /// </summary>
    /// <param name="operation">The operation to apply the filter to.</param>
    /// <param name="context">The context information for the filter.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var odataQuery = context.MethodInfo.GetCustomAttributes(true)
            .OfType<SwaggerODataQueryAttribute>().FirstOrDefault();

        if (odataQuery != null)
        {
            foreach (var param in operation.Parameters)
            {
                if (odataQuery.QueryExamples.TryGetValue(param.Name, out string? example))
                {
                    param.Description += $" Example: {example}";
                }
            }
        }
    }
}
