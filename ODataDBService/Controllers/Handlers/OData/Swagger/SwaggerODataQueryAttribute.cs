// <copyright file="SwaggerODataQueryAttribute.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData.Swagger;

/// <summary>
/// Attribute for adding examples to Swagger documentation for OData query parameters.
/// </summary>
public class SwaggerODataQueryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerODataQueryAttribute"/> class with the specified query parameter examples.
    /// </summary>
    /// <param name="queryExamples">The query parameter examples in the format of "parameter=value".</param>
    public SwaggerODataQueryAttribute(params string[] queryExamples)
    {
        this.QueryExamples = queryExamples
            .Select(example =>
            {
                var keyValue = example.Split(new[] { '=' }, 2);
                return new KeyValuePair<string, string>(keyValue[0], keyValue[1]);
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Gets the dictionary of query parameter names and example values.
    /// </summary>
    public Dictionary<string, string> QueryExamples { get; private set; }
}