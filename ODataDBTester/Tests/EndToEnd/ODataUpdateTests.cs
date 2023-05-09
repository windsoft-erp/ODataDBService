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

public class ODataUpdateTests
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

        _controller = new ODataV4Controller(requestHandlerFactoryMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextAccessorMock.Object.HttpContext
        };
    }
    
    [Test]
    public async Task UpdateAsync_WithValidData_ShouldReturnResponse()
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
        
        var expected = new List<object>
        {
            new Dictionary<string, object> {{"EmployeeID", expectedId}, {"FirstName", "John"}, {"LastName", "Doe"}, {"Title", "Developer"}, {"BirthDate", new DateTime(1990, 1, 1)}, {"HireDate", new DateTime(2022, 1, 1)}, {"City", "New York"}, {"Country", "USA"}, {"TotalOrders", 0}}
        };

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

        using var streamInsert = new MemoryStream(Encoding.UTF8.GetBytes(newEmployeeJson));
        using var jsonDocInsert = await JsonDocument.ParseAsync(streamInsert);
        _controller.HttpContext.Request.Body = streamInsert;
        
        // Act
        var result = await _controller.PostAsync("Employees", jsonDocInsert.RootElement);

        // Assert
        var createdResult = result as CreatedResult;
        Assert.NotNull(createdResult);
        Assert.That(createdResult.StatusCode, Is.EqualTo(201));
        Assert.That(createdResult.Value, Is.EqualTo(expected));
        
        var updateEmployee = @$"
        {{
            ""TotalOrders"": {20}
        }}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(updateEmployee));
        using var jsonDoc = await JsonDocument.ParseAsync(stream);

        result = await _controller.PutAsync("Employees", expectedId.ToString(), jsonDoc.RootElement);
        var okResult = result as OkResult;
        Assert.NotNull(okResult);
        
        result = await _controller.QueryByIdAsync("Employees", expectedId.ToString());
        expected = new List<object>
        {
            new Dictionary<string, object> {{"EmployeeID", expectedId}, {"FirstName", "John"}, {"LastName", "Doe"}, {"Title", "Developer"}, {"BirthDate", new DateTime(1990, 1, 1)}, {"HireDate", new DateTime(2022, 1, 1)}, {"City", "New York"}, {"Country", "USA"}, {"TotalOrders", 20}}
        };
        
        // Assert
        var okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.That(okObjectResult.Value, Is.EqualTo(expected));
        // Cleanup
        await _controller.DeleteAsync("Employees", expectedId.ToString());
    }
    
      
    [Test]
    public async Task UpdateAsync_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var expectedId = 1;

        var updateEmployee = @$"
        {{
            ""TotalOrders"": ""JustAnotherString""
        }}";
        
        using var streamUpdate = new MemoryStream(Encoding.UTF8.GetBytes(updateEmployee));
        using var jsonDocUpdate = await JsonDocument.ParseAsync(streamUpdate);
        // Act
        var result = await _controller.PutAsync("Employees", expectedId.ToString(), jsonDocUpdate.RootElement);
        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.That(badRequestResult.Value, Is.EqualTo($"Could not retrieve record with requested data type, key '{expectedId.ToString()}' from table 'Employees'."));
    }
    
    [Test]
    public async Task UpdateAsync_WithInvalidIdDataType_ShouldBadRequest()
    {
        // Arrange
        var expectedId = "AnotherID";

        var updateEmployee = @$"
        {{
            ""TotalOrders"": {1}
        }}";
        
        using var streamUpdate = new MemoryStream(Encoding.UTF8.GetBytes(updateEmployee));
        using var jsonDocUpdate = await JsonDocument.ParseAsync(streamUpdate);
        // Act
        var result = await _controller.PutAsync("Employees", expectedId, jsonDocUpdate.RootElement);
        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.That(badRequestResult.Value, Is.EqualTo($"Could not retrieve record with requested data type, key '{expectedId.ToString()}' from table 'Employees'."));
    }
    
    [Test]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var expectedId = 100000;

        var updateEmployee = @$"
        {{
            ""TotalOrders"": {1}
        }}";
        
        using var streamUpdate = new MemoryStream(Encoding.UTF8.GetBytes(updateEmployee));
        using var jsonDocUpdate = await JsonDocument.ParseAsync(streamUpdate);
        // Act
        var result = await _controller.PutAsync("Employees", expectedId.ToString(), jsonDocUpdate.RootElement);
        // Assert
        var notFoundObjectResult = result as NotFoundObjectResult;
        Assert.NotNull(notFoundObjectResult);
        Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Could not retrieve record with key '{expectedId}' from table 'Employees' for updating."));
    }
    
    [Test]
    public async Task UpdateAsync_WithInvalidTable_ShouldReturnNotFound()
    {
        // Arrange
        var expectedId = 100000;
        var tableName = "FakeTable";

        var updateEmployee = @$"
        {{
            ""TotalOrders"": {1}
        }}";
        
        using var streamUpdate = new MemoryStream(Encoding.UTF8.GetBytes(updateEmployee));
        using var jsonDocUpdate = await JsonDocument.ParseAsync(streamUpdate);
        // Act
        var result = await _controller.PutAsync(tableName, expectedId.ToString(), jsonDocUpdate.RootElement);
        // Assert
        var notFoundObjectResult = result as NotFoundObjectResult;
        Assert.NotNull(notFoundObjectResult);
        Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Could not retrieve table '{tableName}' with primary key of requested data type."));
    }
}