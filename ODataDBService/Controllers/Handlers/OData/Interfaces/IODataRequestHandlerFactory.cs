// <copyright file="IODataRequestHandlerFactory.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData.Interfaces;

/// <summary>
/// Defines a contract for creating multiple types of requests methods.
/// </summary>
public interface IODataRequestHandlerFactory
{
    IQueryRequestHandler CreateQueryHandler();

    IQueryByIdRequestHandler CreateQueryByIdHandler();

    IDeleteRequestHandler CreateDeleteHandler();

    IInsertRequestHandler CreateInsertHandler();

    IUpdateRequestHandler CreateUpdateHandler();

    IInvalidateCacheRequestHandler CreateInvalidateCacheHandler();

    IBatchRequestHandler CreateBatchHandler();
}