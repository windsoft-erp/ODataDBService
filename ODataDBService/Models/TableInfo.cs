// <copyright file="TableInfo.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Models;

/// <summary>
/// Represents information about a table in the database.
/// </summary>
public class TableInfo
{
    /// <summary>
    /// Gets or initializes the name of the table.
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Gets or sets the primary key of the table.
    /// </summary>
    public string? PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the names of the columns in the table.
    /// </summary>
    public IEnumerable<string>? ColumnNames { get; set; }
}