using ODataDBService.Controllers.Handlers.OData.Interfaces;

public class ODataRequestHandlerFactory : IODataRequestHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ODataRequestHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider=serviceProvider;
    }

    public IQueryRequestHandler CreateQueryHandler() => _serviceProvider.GetRequiredService<IQueryRequestHandler>();
    public IDeleteRequestHandler CreateDeleteHandler() => _serviceProvider.GetRequiredService<IDeleteRequestHandler>();
    public IInsertRequestHandler CreateInsertHandler() => _serviceProvider.GetRequiredService<IInsertRequestHandler>();
    public IUpdateRequestHandler CreateUpdateHandler() => _serviceProvider.GetRequiredService<IUpdateRequestHandler>();
    public IInvalidateCacheRequestHandler CreateInvalidateCacheHandler() =>_serviceProvider.GetRequiredService<IInvalidateCacheRequestHandler>();
    public IBatchRequestHandler CreateBatchRequestHandler() => _serviceProvider.GetRequiredService<IBatchRequestHandler>(); 
}