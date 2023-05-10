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

public class ODataDelete
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

        var requestHandlerFactoryMock = new Mock<IODataRequestHandlerFactory>();

        var urlHelperMock = new Mock<IUrlHelper>();
        var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        urlHelperFactoryMock.Setup(factory => factory.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelperMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(new DefaultHttpContext());

        requestHandlerFactoryMock.Setup(factory => factory.CreateInsertHandler()).Returns(
            new InsertRequestHandler(loggerInsert, _oDataV4Service));

        requestHandlerFactoryMock.Setup(factory => factory.CreateDeleteHandler()).Returns(
            new DeleteRequestHandler(loggerDelete, _oDataV4Service));

        _controller = new ODataV4Controller(requestHandlerFactoryMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextAccessorMock.Object.HttpContext
        };
    }
    
    [Test]
    public async Task DeleteAsync_ValidData_ShouldReturnOk()
    {
        // Arrange
        var expectedId = new Random().Next(1000, 5000);
        var expectedFirstName = "John";
        var expectedLastName = "Doe";
        var expectedTitle = "Developer";
        var expectedBirthDate = new DateTime(1990, 1, 1);
        var expectedHireDate = new DateTime(2022, 1, 1);
        var expectedCity = "New York";
        var expectedCountry = "USA";
        var expectedTotalOrders = 0;

        var newEmployeeJson = @$"
        {{
            ""EmployeeID"": {expectedId},
            ""FirstName"": ""{expectedFirstName}"",
            ""LastName"": ""{expectedLastName}"",
            ""Title"": ""{expectedTitle}"",
            ""BirthDate"": ""{expectedBirthDate:yyyy-MM-dd}"",
            ""HireDate"": ""{expectedHireDate:yyyy-MM-dd}"",
            ""City"": ""{expectedCity}"",
            ""Country"": ""{expectedCountry}"",
            ""TotalOrders"": {expectedTotalOrders}
        }}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(newEmployeeJson));
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        _controller.HttpContext.Request.Body = stream;
        
        // Act
        await _controller.PostAsync("Employees", jsonDoc.RootElement);

        // Assert
        var result = await _controller.DeleteAsync("Employees", expectedId.ToString());
        var deleteResult = result as OkResult;
        Assert.NotNull(deleteResult);
    }
    
    [Test]
    public async Task DeleteAsync_InvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var tableName = "Employees";
        var expectedId = new Random().Next(1000, 5000);

        // Assert
        var result = await _controller.DeleteAsync(tableName, expectedId.ToString());
        var notFoundObjectResult = result as NotFoundObjectResult;
        Assert.NotNull(notFoundObjectResult);
        Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Could not retrieve record with key '{expectedId.ToString()}' from table '{tableName}' for deletion."));
    }
    
    [Test]
    public async Task DeleteAsync_InvalidIdDataType_ShouldReturnNotFound()
    {
        // Arrange
        var tableName = "Employees";
        var expectedId = "Hello";

        // Assert
        var result = await _controller.DeleteAsync(tableName, expectedId.ToString());
        var BadRequestObjectResult = result as BadRequestObjectResult;
        Assert.NotNull(BadRequestObjectResult);
        Assert.That(BadRequestObjectResult.Value, Is.EqualTo($"Could not retrieve record with requested data type, key '{expectedId}' from table '{tableName}'."));
    }
    
    [Test]
    public async Task DeleteAsync_InvalidTable_ShouldReturnNotFound()
    {
        // Arrange
        var tableName = "FakeTable";
        var expectedId = new Random().Next(1000, 5000);

        // Assert
        var result = await _controller.DeleteAsync(tableName, expectedId.ToString());
        var notFoundObjectResult = result as NotFoundObjectResult;
        Assert.NotNull(notFoundObjectResult);
        Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Could not retrieve table '{tableName}' with primary key of requested data type."));
    }
}