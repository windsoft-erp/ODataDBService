namespace ODataDBService.Controllers.Handlers.OData.Interfaces
{
    public interface IODataRequestHandlerFactory
    {
        IQueryRequestHandler CreateQueryHandler();
        IDeleteRequestHandler CreateDeleteHandler();
        IInsertRequestHandler CreateInsertHandler();
        IUpdateRequestHandler CreateUpdateHandler();
        IInvalidateCacheRequestHandler CreateInvalidateCacheHandler();
        IBatchRequestHandler CreateBatchRequestHandler();
    }
}
