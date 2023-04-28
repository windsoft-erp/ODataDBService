using ODataDBService.Controllers.Handlers.OData.Interfaces;

public class ODataRequestHandlerFactory : IODataRequestHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ODataRequestHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider=serviceProvider;
    }

    public IQueryRequestHandler CreateQueryHandler()
    {
        return _serviceProvider.GetRequiredService<IQueryRequestHandler>();
    }

    public IDeleteRequestHandler CreateDeleteHandler()
    {
        return _serviceProvider.GetRequiredService<IDeleteRequestHandler>();
    }

    public IInsertRequestHandler CreateInsertHandler()
    {
        return _serviceProvider.GetRequiredService<IInsertRequestHandler>();
    }

    public IUpdateRequestHandler CreateUpdateHandler()
    {
        return _serviceProvider.GetRequiredService<IUpdateRequestHandler>();
    }

    public IInvalidateCacheRequestHandler CreateInvalidateCacheHandler()
    {
        return _serviceProvider.GetRequiredService<IInvalidateCacheRequestHandler>();
    }
}