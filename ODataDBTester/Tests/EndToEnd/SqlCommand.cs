using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ODataDBService.Controllers;
using ODataDBService.Controllers.Handlers.SqlCommand;
using ODataDBService.Services;
using ODataDBService.Services.Repositories;

namespace ODataDBTester.Tests.EndToEnd;

public class SqlCommand
{
    private SqlCommandRepository _sqlCommandRepository;
    private SqlCommandService _sqlCommandService;
    private SqlCommandController _controller;

    [SetUp]
    public void Setup()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        _sqlCommandRepository = new SqlCommandRepository(config);
        _sqlCommandService = new SqlCommandService(_sqlCommandRepository);
        var logger = Mock.Of<ILogger<StoredProcedureRequestHandler>>();

        StoredProcedureRequestHandler spHandler = new StoredProcedureRequestHandler(logger, _sqlCommandService);

        _controller = new SqlCommandController(spHandler);
    }
    
    [Test]
    public async Task ExecuteStoredProcedureAsync_ValidInput_ReturnsOkResult()
    {
        // arrange
        var procedureParameters = new
        {
            EmployeeID = 15,
            OrderDate = new DateTime(2023, 05, 10),
            ShipName = "Test Ship Name",
            ShipCity = "Test Ship City",
            ShipCountry = "Test Ship Country",
            Value = 100
        };
        var json = JsonSerializer.Serialize(procedureParameters);
        var expected = new OkObjectResult("{\"result\":1}");

        // act
        var actual = await _controller.ExecuteStoredProcedureAsync("InsertOrderAndUpdateEmployee", JsonDocument.Parse(json).RootElement);

        // assert
        var okObject = actual as OkResult;
        Assert.IsNotNull(okObject);
    }

    [Test] 
    public async Task ExecuteStoredProcedureAsync_ValidInput_ReturnsOkResultResult()
    {
        // arrange
        var procedureParameters = new
        {
            City = "London"
        };
        var json = JsonSerializer.Serialize(procedureParameters);

        // act
        var actual = await _controller.ExecuteStoredProcedureAsync("GetEmployeesByCity", JsonDocument.Parse(json).RootElement);

        // assert
        var okObject = actual as OkObjectResult;
        Assert.That(okObject, Is.Not.Null);
        Assert.That(okObject.Value, Is.InstanceOf<IEnumerable<dynamic>>());

        var employees = okObject.Value as IEnumerable<dynamic>;
        Assert.That(employees, Is.Not.Null);
        Assert.That(employees, Has.Exactly(5).Items);
    }
    
    [Test]
    public async Task ExecuteStoredProcedureAsync_ValidInputButInvalidSPParam_ReturnsBadRequest()
    {
        // arrange
        var procedureParameters = new
        {
            EmployeeID = 100000000,
            OrderDate = new DateTime(2023, 05, 10),
            ShipName = "Test Ship Name",
            ShipCity = "Test Ship City",
            ShipCountry = "Test Ship Country",
            Value = 100
        };
        var json = JsonSerializer.Serialize(procedureParameters);
        var expected = new OkObjectResult("{\"result\":1}");

        // act
        var actual = await _controller.ExecuteStoredProcedureAsync("InsertOrderAndUpdateEmployee", JsonDocument.Parse(json).RootElement);

        // assert
        var badRequest = actual as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.That(badRequest.Value, Is.EqualTo($"The stored procedure 'InsertOrderAndUpdateEmployee' has thrown an error."));
    }
    
    [Test]
    public async Task ExecuteStoredProcedureAsync_InvalidInputData_ReturnBadRequest()
    {
        // arrange
        var procedureParameters = new
        {
            EmployeeID = 1,
            OrderDate = "This is clearly not a DateTime",
            ShipName = "Test Ship Name",
            ShipCity = "Test Ship City",
            ShipCountry = "Test Ship Country",
            Value = 100
        };
        var json = JsonSerializer.Serialize(procedureParameters);
        var expected = new OkObjectResult("{\"result\":1}");

        // act
        var actual = await _controller.ExecuteStoredProcedureAsync("InsertOrderAndUpdateEmployee", JsonDocument.Parse(json).RootElement);

        // assert
        var badRequest = actual as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.That(badRequest.Value, Is.EqualTo($"Corrupted data: Cannot convert 'This is clearly not a DateTime' to DateTime for procedure parameter '@OrderDate'."));
    }
    
    [Test]
    public async Task ExecuteStoredProcedureAsync_InvalidSPName_ReturnsNotFound()
    {
        // arrange
        var procedureParameters = new
        {
            EmployeeID = 1,
            OrderDate = new DateTime(2023, 05, 10),
            ShipName = "Test Ship Name",
            ShipCity = "Test Ship City",
            ShipCountry = "Test Ship Country",
            Value = 100
        };
        var json = JsonSerializer.Serialize(procedureParameters);
        var expected = new OkObjectResult("{\"result\":1}");

        // act
        var actual = await _controller.ExecuteStoredProcedureAsync("WhatIsThisSPName", JsonDocument.Parse(json).RootElement);

        // assert
        var notFoundObjectResult = actual as NotFoundObjectResult;
        Assert.NotNull(notFoundObjectResult);
        Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Could not find stored procedure: WhatIsThisSPName."));
    }
}