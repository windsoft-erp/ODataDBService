// <copyright file="RequestHandlerConstants.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ODataDBService.Utilities.Constants;

internal static class RequestHandlerConstants
{
    public const string NotFoundMessageFormat = "Could not retrieve record with key '{0}' from table '{1}'.";
    public const string BadRequestMessageDataTypeFormat = "Could not retrieve record with requested data type, key '{0}' from table '{1}'.";
    public const string NotFoundMessagePrimaryKeyFormat = "Could not retrieve table '{0}' with primary key of requested data type.";
    public const string QueryByIdSuccessMessageFormat = "Successfully retrieved record from table '{0}'.";
    public const string QueryByIdErrorMessageFormat = "Error retrieving record with key '{0}' from table '{1}'.";
    public const string QueryInvalidParametersFormat = "Invalid parameters in query string: {0}.";
    public const string QueryNoContentFormat = "No records found for '{0}'.";
    public const string QueryTableNotFoundFormat = "Table '{0}' does not exist.";
    public const string QuerySuccessMessageFormat = "Successfully retrieved records for '{0}'.";
    public const string QueryErrorMessageFormat = "Error retrieving records from '{0}.'";
    public const string UpdateSuccessMessageFormat = "Successfully updated record with key '{0}' in table '{1}'";
    public const string UpdateErrorMessageFormat = "Error updating record with key '{0}' from table '{1}'.";
    public const string UpdateNotFoundMessageFormat = "Could not retrieve record with key '{0}' from table '{1}' for updating.";
    public const string InsertSuccessMessageFormat = "Successfully inserted record into table '{0}'.";
    public const string InsertNotFoundMessageFormat = "Error inserting record into table '{0}'.";
    public const string InsertBadRequestPkViolationMessageFormat = "Error inserting the record into '{0}', PRIMARY KEY violation.";
    public const string InsertBadRequestDataTypeErrorMessageFormat = "Error inserting the record into '{0}', corrupted data present in request body.";
    public const string DeleteSuccessMessageFormat = "Successfully deleted record with key '{0}' from table '{1}'.";
    public const string DeleteNotFoundMessageFormat = "Could not retrieve record with key '{0}' from table '{1}' for deletion.";
    public const string DeleteErrorMessageFormat = "Error deleting record with key '{0}' from table '{1}'.";
    public const string InvalidateCacheSuccessMessageFormat = "Table info cache for '{0}' has been invalidated.";
    public const string InvalidateCacheNotFoundMessageFormat = "Table info '{0}' not found in the cache.";
    public const string SqlExceptionConversionFailed = "Conversion failed when converting";
    public const string SqlExceptionPkViolation = "Violation of PRIMARY KEY constraint";
    public const string SqlExceptionTableNotFound = "Invalid object name '{0}'";
    public const string SqlExceptionInvalidColumnName = "Invalid column name";
    public const string ArgumentExceptionNoPrimaryKeyFormat = "Could not find primary key for table {0}";
}
