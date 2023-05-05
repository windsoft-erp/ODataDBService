// <copyright file="StoredProcedureParameter.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Models;

/// <summary>
/// Represents a stored procedure parameter.
/// </summary>
public class StoredProcedureParameter
{
    /// <summary>
    /// Gets or sets the name of the stored procedure parameter.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the stored procedure parameter.
    /// </summary>
    public string? Type { get; set; }
}
