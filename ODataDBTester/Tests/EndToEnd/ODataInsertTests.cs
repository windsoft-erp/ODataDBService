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
using ODataDBTester.Helpers;
using SqlKata.Compilers;

namespace ODataDBTester.Tests.EndToEnd;

[TestFixture]
public class ODataInsertTests
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
    public async Task InsertAsyncWithValidData_ShouldCreateNewEntry()
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

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(newEmployeeJson));
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        _controller.HttpContext.Request.Body = stream;
        
        // Act
        var result = await _controller.PostAsync("Employees", jsonDoc.RootElement);

        // Assert
        var createdResult = result as CreatedResult;
        Assert.NotNull(createdResult);
        Assert.That(createdResult.StatusCode, Is.EqualTo(201));
        Assert.That(createdResult.Value, Is.EqualTo(expected));
        
        // Cleanup
        await _controller.DeleteAsync("Employees", expectedId.ToString());
    }
    
    
    
        [Test]
    public async Task InsertAsyncWithValidDataOnTableWithNoId_ShouldCreateNewEntry()
    {
        // Arrange
        var expectedCustomerName = "John";
        var expectedOrderDate = new DateTime(2023, 1, 1);
        var expectedOrderTotal = 3;
        
        var expected = new
        {
            CustomerName = expectedCustomerName,
            OrderDate = expectedOrderDate,
            OrderTotal = expectedOrderTotal
        };

        var newOrder = @$"
        {{
            ""CustomerName"": ""{expectedCustomerName}"",
            ""OrderDate"": ""{expectedOrderDate}"",
            ""OrderTotal"": {expectedOrderTotal}
        }}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(newOrder));
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        _controller.HttpContext.Request.Body = stream;
        
        // Act
        var result = await _controller.PostAsync("CustomerOrder", jsonDoc.RootElement);

        // Assert
        var createdResult = result as CreatedResult;
        Assert.NotNull(createdResult);
        Assert.That(createdResult.StatusCode, Is.EqualTo(201));
        Assert.That(createdResult.Value, Is.EqualTo(expected).Using(new PropertyComparer<dynamic>()));  // using CompareObjects comparer
    }
    
        [Test]
    public async Task InsertAsyncSameEntryTwice_ShouldReturnBadRequest()
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

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(newEmployeeJson));
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        _controller.HttpContext.Request.Body = stream;
        
        // Act
        var result = await _controller.PostAsync("Employees", jsonDoc.RootElement);

        // Assert
        var createdResult = result as CreatedResult;
        Assert.NotNull(createdResult);
        Assert.That(createdResult.StatusCode, Is.EqualTo(201));
        Assert.That(createdResult.Value, Is.EqualTo(expected));
        
        result = await _controller.PostAsync("Employees", jsonDoc.RootElement);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.That(badRequestResult.Value, Is.EqualTo($"Error inserting the record into 'Employees', PRIMARY KEY violation."));
        
        // Cleanup
        await _controller.DeleteAsync("Employees", expectedId.ToString());
    }
    
    [Test]
    public async Task InsertAsyncWithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var expectedId = new Random().Next(1000, 5000);
        var expectedFirstName = "John";
        var expectedLastName = "Doe";
        var expectedTitle = "Developer";
        var expectedBirthDate = "Corrupted Date";
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
            ""BirthDate"": ""{expectedBirthDate}"",
            ""HireDate"": ""{expectedHireDate:yyyy-MM-dd}"",
            ""City"": ""{expectedCity}"",
            ""Country"": ""{expectedCountry}"",
            ""TotalOrders"": {expectedTotalOrders}
        }}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(newEmployeeJson));
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        _controller.HttpContext.Request.Body = stream;
        
        // Act
        var result = await _controller.PostAsync("Employees", jsonDoc.RootElement);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.That(badRequestResult.Value, Is.EqualTo($"Error inserting the record into 'Employees', corrupted data present in request body."));
    }
    
    [Test]
    public async Task InsertAsyncInvalidTable_ShouldReturnNotFound()
    {
        // Arrange
        var expectedId = new Random().Next(1000, 5000);
        var expectedFirstName = "John";
        var expectedLastName = "Doe";
        var expectedTitle = "Developer";
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
            ""HireDate"": ""{expectedHireDate:yyyy-MM-dd}"",
            ""City"": ""{expectedCity}"",
            ""Country"": ""{expectedCountry}"",
            ""TotalOrders"": {expectedTotalOrders}
        }}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(newEmployeeJson));
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        _controller.HttpContext.Request.Body = stream;
        
        // Act
        var result = await _controller.PostAsync("AnotherFakeTable", jsonDoc.RootElement);

        // Assert
        var notFoundObjectResult = result as NotFoundObjectResult;
        Assert.NotNull(notFoundObjectResult);
        Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Table 'AnotherFakeTable' does not exist."));
    }
}