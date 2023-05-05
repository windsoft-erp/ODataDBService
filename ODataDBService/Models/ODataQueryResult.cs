// <copyright file="ODataQueryResult.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Models;
using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the result of an OData query.
/// </summary>
public class ODataQueryResult
{
    /// <summary>
    /// Gets or initializes the count of records in the result set.
    /// </summary>
    [Description("Count of records in result set")]
    public int Count { get; init; }

    /// <summary>
    /// Gets or sets the URL to fetch the next set of records.
    /// Example: For skip=0 and top=10, this will contain a link to retrieve 10 records after skipping the first 10 records.
    /// The <see cref="NextLink"/> property will be null if this is the last set of records.
    /// </summary>
    [Description("URL to fetch next set of records. Example : For skip = 0 and top = 10, this will contain link to retrieve 10 records after skipping first 10 records." +
        "\n NextLink will be null if this is the last set of records")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NextLink { get; set; }

    /// <summary>
    /// Gets or initializes the records in the current set.
    /// </summary>
    [Description("Records in current set")]
    public IEnumerable<dynamic>? Value { get; init; }
}
