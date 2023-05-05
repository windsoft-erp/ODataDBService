// <copyright file="ODataQuery.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Models;

/// <summary>
/// This class represents an OData query and its associated properties.
/// </summary>
public class ODataQuery
{
    /// <summary>
    /// Gets or initializes the name of the table to be queried.
    /// </summary>
    public string TableName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the columns to be returned by the query.
    /// </summary>
    public string Select { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the filter criteria for the query.
    /// </summary>
    public string Filter { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the apply clause for the query.
    /// </summary>
    public string Apply { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the order in which the results are returned.
    /// </summary>
    public string OrderBy { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the maximum number of results to be returned.
    /// </summary>
    public int Top { get; init; }

    /// <summary>
    /// Gets or initializes the number of results to skip before returning data.
    /// </summary>
    public int Skip { get; init; }
}