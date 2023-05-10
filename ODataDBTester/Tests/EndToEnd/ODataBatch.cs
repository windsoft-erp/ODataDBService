using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DynamicODataToSQL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ODataDBService.Controllers;
using ODataDBService.Controllers.Handlers.OData;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Services;
using ODataDBService.Services.Repositories;
using SqlKata.Compilers;

namespace ODataDBTester.Tests.EndToEnd;

public class ODataBatch
{
    private ODataV4Repository _oDataV4Repository;
    private ODataV4Service _oDataV4Service;
    private ODataV4Controller _controller;
    private EdmModelBuilder _edmModelBuilder;
    private SqlServerCompiler _sqlServerCompiler;
    private ODataToSqlConverter _oDataToSqlConverter;

    [SetUp]
    public void Setup()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        _edmModelBuilder = new EdmModelBuilder();
        _sqlServerCompiler = new SqlServerCompiler { UseLegacyPagination = false };
        _oDataToSqlConverter = new ODataToSqlConverter(_edmModelBuilder, _sqlServerCompiler);
        _oDataV4Repository = new ODataV4Repository(config, _oDataToSqlConverter);
        _oDataV4Service = new ODataV4Service(_oDataV4Repository);
        var loggerInsert = Mock.Of<ILogger<InsertRequestHandler>>();
        var loggerDelete = Mock.Of<ILogger<DeleteRequestHandler>>();
        var loggerUpdate =  Mock.Of<ILogger<UpdateRequestHandler>>();
        var loggerById = Mock.Of<ILogger<QueryByIdRequestHandler>>();
        var loggerBatch = Mock.Of<ILogger<BatchRequestHandler>>();

        var requestHandlerFactoryMock = new Mock<IODataRequestHandlerFactory>();

        var urlHelperMock = new Mock<IUrlHelper>();
        var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        urlHelperFactoryMock.Setup(factory => factory.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelperMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(new DefaultHttpContext());
        
        requestHandlerFactoryMock.Setup(factory => factory.CreateUpdateHandler()).Returns(
            new UpdateRequestHandler(loggerUpdate, _oDataV4Service));

        requestHandlerFactoryMock.Setup(factory => factory.CreateInsertHandler()).Returns(
            new InsertRequestHandler(loggerInsert, _oDataV4Service));
        
        requestHandlerFactoryMock.Setup(factory => factory.CreateDeleteHandler()).Returns(
            new DeleteRequestHandler(loggerDelete, _oDataV4Service));
        
        requestHandlerFactoryMock.Setup(factory => factory.CreateQueryByIdHandler()).Returns(
            new QueryByIdRequestHandler(loggerById, _oDataV4Service));
        
        requestHandlerFactoryMock.Setup(factory => factory.CreateBatchHandler()).Returns(
            new BatchRequestHandler(loggerBatch, requestHandlerFactoryMock.Object));

        _controller = new ODataV4Controller(requestHandlerFactoryMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextAccessorMock.Object.HttpContext
        };
    }
    [Test]
    public async Task BatchEndpoint_WithValidData_ShouldReturnExpectedResults()
    {
        // Arrange
        var batchBoundary = "batch_boundary";
        var batchContent = new StringBuilder()
            .AppendLine($"--{batchBoundary}")
            .AppendLine("Content-Type: application/http")
            .AppendLine("Content-Transfer-Encoding: binary")
            .AppendLine()
            .AppendLine("GET /ODataV4/Employees(1) HTTP/1.1")
            .AppendLine($"--{batchBoundary}--")
            .ToString();

        var multipartContent = new MultipartContent("mixed", batchBoundary);
        multipartContent.Add(new StringContent(batchContent));

        var request = new HttpRequestMessage(HttpMethod.Post, "/ODataV4/$batch");
        request.Content = multipartContent;

        _controller.ControllerContext.HttpContext.Request.Body = await request.Content.ReadAsStreamAsync();
        _controller.ControllerContext.HttpContext.Request.ContentType = request.Content.Headers.ContentType.ToString();

        // Act
        var result = await _controller.BatchAsync(null);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
    }
}