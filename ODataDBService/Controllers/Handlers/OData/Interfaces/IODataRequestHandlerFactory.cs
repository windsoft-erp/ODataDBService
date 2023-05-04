namespace ODataDBService.Controllers.Handlers.OData.Interfaces;
public interface IODataRequestHandlerFactory
{
    IQueryRequestHandler CreateQueryHandler();
    IQueryByIdRequestHandler CreateQueryByIdRequestHandler();
    IDeleteRequestHandler CreateDeleteHandler();
    IInsertRequestHandler CreateInsertHandler();
    IUpdateRequestHandler CreateUpdateHandler();
    IInvalidateCacheRequestHandler CreateInvalidateCacheHandler();
    IBatchRequestHandler CreateBatchRequestHandler();
}

