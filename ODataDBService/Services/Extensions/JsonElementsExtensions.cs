// <copyright file="JsonElementsExtensions.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Services.Extensions;
using System.Text.Json;

/// <summary>
/// Extension methods for <see cref="JsonElement"/>.
/// </summary>
public static class JsonElementsExtensions
{
    /// <summary>
    /// Converts a <see cref="JsonElement"/> to an object of type <see cref="object"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> to convert.</param>
    /// <returns>An object representing the value of the <see cref="JsonElement"/>.</returns>
    public static object? ToObject(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when element.TryGetDouble(out var doubleValue) => doubleValue,
            JsonValueKind.Number when element.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.GetRawText(),
            JsonValueKind.Object => element.GetRawText(),
            _ => throw new ArgumentOutOfRangeException($"Unknown JsonElement ValueKind: {element.ValueKind}"),
        };
    }
}