// <copyright file="ODataRequestHandlerFactory.cs" company="WindSoft">
// Copyright (c) WindSoft. All rights reserved.
// Licensed under the WindSoft license. See LICENSE file in the project root for full license information.
// </copyright>
namespace ODataDBService.Controllers.Handlers.OData;
using Interfaces;

/// <summary>
/// Factory for creating OData request handlers.
/// </summary>
public class ODataRequestHandlerFactory : IODataRequestHandlerFactory
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataRequestHandlerFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ODataRequestHandlerFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="IQueryRequestHandler"/> class.
    /// </summary>
    /// <returns>The new <see cref="IQueryRequestHandler"/> instance.</returns>
    public IQueryRequestHandler CreateQueryHandler() => this.serviceProvider.GetRequiredService<IQueryRequestHandler>();

    /// <summary>
    /// Creates a new instance of the <see cref="IQueryByIdRequestHandler"/> class.
    /// </summary>
    /// <returns>The new <see cref="IQueryByIdRequestHandler"/> instance.</returns>
    public IQueryByIdRequestHandler CreateQueryByIdHandler() => this.serviceProvider.GetRequiredService<IQueryByIdRequestHandler>();

    /// <summary>
    /// Creates a new instance of the <see cref="IDeleteRequestHandler"/> class.
    /// </summary>
    /// <returns>The new <see cref="IDeleteRequestHandler"/> instance.</returns>
    public IDeleteRequestHandler CreateDeleteHandler() => this.serviceProvider.GetRequiredService<IDeleteRequestHandler>();

    /// <summary>
    /// Creates a new instance of the <see cref="IInsertRequestHandler"/> class.
    /// </summary>
    /// <returns>The new <see cref="IInsertRequestHandler"/> instance.</returns>
    public IInsertRequestHandler CreateInsertHandler() => this.serviceProvider.GetRequiredService<IInsertRequestHandler>();

    /// <summary>
    /// Creates a new instance of the <see cref="IUpdateRequestHandler"/> class.
    /// </summary>
    /// <returns>The new <see cref="IUpdateRequestHandler"/> instance.</returns>
    public IUpdateRequestHandler CreateUpdateHandler() => this.serviceProvider.GetRequiredService<IUpdateRequestHandler>();

    /// <summary>
    /// Creates a new instance of the <see cref="IInvalidateCacheRequestHandler"/> class.
    /// </summary>
    /// <returns>The new <see cref="IInvalidateCacheRequestHandler"/> instance.</returns>
    public IInvalidateCacheRequestHandler CreateInvalidateCacheHandler() => this.serviceProvider.GetRequiredService<IInvalidateCacheRequestHandler>();

    /// <summary>
    /// Creates a new instance of the <see cref="IBatchRequestHandler"/> class.
    /// </summary>
    /// <returns>The new <see cref="IBatchRequestHandler"/> instance.</returns>
    public IBatchRequestHandler CreateBatchHandler() => this.serviceProvider.GetRequiredService<IBatchRequestHandler>();
}