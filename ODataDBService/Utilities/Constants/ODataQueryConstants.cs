// <copyright file="ODataQueryConstants.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Utilities.Constants;

internal static class ODataQueryConstants
{
    public static class QueryString
    {
        public const string Select = "$select";
        public const string Filter = "$filter";
        public const string OrderBy = "$orderby";
        public const string Top = "$top";
        public const string Skip = "$skip";
        public const string Apply = "$apply";
    }
}