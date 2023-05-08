using DynamicODataToSQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ODataDBService.Controllers;
using ODataDBService.Services.Repositories;
using ODataDBService.Services;
using SqlKata.Compilers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using ODataDBService.Models;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Controllers.Handlers.OData;
using Microsoft.Extensions.Primitives;

namespace ODataDBTester.Tests.EndToEnd
{
    [TestFixture]
    public class ODataQueryTests
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
            var logger = Mock.Of<ILogger<QueryRequestHandler>>();
            var loggerById = Mock.Of<ILogger<QueryByIdRequestHandler>>();

            var requestHandlerFactoryMock = new Mock<IODataRequestHandlerFactory>();

            var urlHelperMock = new Mock<IUrlHelper>();
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock.Setup(factory => factory.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(new DefaultHttpContext());

            requestHandlerFactoryMock.Setup(factory => factory.CreateQueryHandler()).Returns(new QueryRequestHandler(logger, _oDataV4Service, httpContextAccessorMock.Object, urlHelperFactoryMock.Object));
            
            requestHandlerFactoryMock.Setup(factory => factory.CreateQueryByIdHandler()).Returns(
                new QueryByIdRequestHandler(loggerById, _oDataV4Service));

            _controller = new ODataV4Controller(requestHandlerFactoryMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            };
        }

        [Test]
        public async Task QueryAsync_WithValidParameters_ShouldReturnODataQueryResult()
        {
            // Arrange
            var tableName = "Employees";
            var expected = new List<object>
            {
                new Dictionary<string, object> {{"EmployeeID", 1}, {"FirstName", "Nancy"}, {"LastName", "Davolio"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1968, 12, 8)}, {"HireDate", new DateTime(1992, 5, 1)}, {"City", "Seattle"}, {"Country", "USA"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 2}, {"FirstName", "Andrew"}, {"LastName", "Fuller"}, {"Title", "Vice President, Sales"}, {"BirthDate", new DateTime(1972, 2, 19)}, {"HireDate", new DateTime(1992, 8, 14)}, {"City", "Tacoma"}, {"Country", "USA"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 3}, {"FirstName", "Janet"}, {"LastName", "Leverling"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1985, 8, 30)}, {"HireDate", new DateTime(1992, 4, 1)}, {"City", "Kirkland"}, {"Country", "USA"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 4}, {"FirstName", "Margaret"}, {"LastName", "Peacock"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1973, 9, 19)}, {"HireDate", new DateTime(1993, 5, 3)}, {"City", "Redmond"}, {"Country", "USA"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 5}, {"FirstName", "Steven"}, {"LastName", "Buchanan"}, {"Title", "Sales Manager"}, {"BirthDate", new DateTime(1955, 3, 4)}, {"HireDate", new DateTime(1993, 10, 17)}, {"City", "London"}, {"Country", "UK"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 6}, {"FirstName", "Michael"}, {"LastName", "Suyama"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1983, 7, 2)}, {"HireDate", new DateTime(1993, 10, 17)}, {"City", "London"}, {"Country", "UK"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 7}, {"FirstName", "Robert"}, {"LastName", "King"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1960, 5, 29)}, {"HireDate", new DateTime(1994, 1, 2)}, {"City", "London"}, {"Country", "UK"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 8}, {"FirstName", "Laura"}, {"LastName", "Callahan"}, {"Title", "Inside Sales Coordinator"}, {"BirthDate", new DateTime(1978, 1, 9)}, {"HireDate", new DateTime(1994, 3, 5)}, {"City", "Seattle"}, {"Country", "USA"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 9}, {"FirstName", "Anne"}, {"LastName", "Dodsworth"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1986, 1, 27)}, {"HireDate", new DateTime(1994, 11, 15)}, {"City", "London"}, {"Country", "UK"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 10}, {"FirstName", "Andrew"}, {"LastName", "Smith"}, {"Title", "Marketing Manager"}, {"BirthDate", new DateTime(1972, 5, 16)}, {"HireDate", new DateTime(1995, 1, 1)}, {"City", "New York"}, {"Country", "USA"}, {"TotalOrders", 0}},
                new Dictionary<string, object> {{"EmployeeID", 11}, {"FirstName", "Bradley"}, {"LastName", "Zimmer"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1975, 1, 18)}, {"HireDate", new DateTime(1995, 2, 1)}, {"City", "New York"}, {"Country", "USA"}, {"TotalOrders", 0}}
            };

            // Act
            var result = await _controller.QueryAsync(tableName);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            var actual = okResult.Value as ODataQueryResult;
            Assert.That(actual.Value, Is.EqualTo(expected));
        }

        [Test]
        public async Task QueryAsync_WithFilterByFirstName_ShouldReturnFilteredResults()
        {
            // Arrange
            var tableName = "Employees";
            var expected = new List<object>
            {
                new Dictionary<string, object> {{"EmployeeID", 1}, {"FirstName", "Nancy"}, {"LastName", "Davolio"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1968, 12, 8)}, {"HireDate", new DateTime(1992, 5, 1)}, {"City", "Seattle"}, {"Country", "USA"}, {"TotalOrders", 0}}
            };

            var queryString = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "$filter", new StringValues("FirstName eq 'Nancy'") }
            });

            _controller.HttpContext.Request.Query=queryString;

            // Act
            var result = await _controller.QueryAsync(tableName);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            var actual = okResult.Value as ODataQueryResult;
            Assert.That(actual.Value, Is.EqualTo(expected));
        }


        [Test]
        public async Task QueryAsync_OnNonExistingColumn_ShouldReturnNotFound()
        {
            // Arrange
            var tableName = "Employees";

            var queryString = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "$select", new StringValues("AnotherColumn") }
            });

            _controller.HttpContext.Request.Query=queryString;

            // Act
            var result = await _controller.QueryAsync(tableName);

            // Assert
            var notFoundObjectResult = result as NotFoundObjectResult;
            Assert.NotNull(notFoundObjectResult);
            Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Invalid column name 'AnotherColumn'."));
        }

        [Test]
        public async Task QueryAsync_WithEmptyTable_ShouldReturnNoContent()
        {
            var tableName = "Products";

            // Act
            var result = await _controller.QueryAsync(tableName);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.NotNull(noContentResult);
        }

        [Test]
        public async Task QueryAsync_OnNonExistingTable_ShouldReturnNotFound()
        {   
            var tableName = "InvalidTable";

            // Act
            var result = await _controller.QueryAsync(tableName);

            // Assert
            var notFoundObjectResult = result as NotFoundObjectResult;
            Assert.NotNull(notFoundObjectResult);
            Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Table '{tableName}' does not exist."));
        }

        [Test]
        public async Task QueryAsync_WithInvalidLeftSideParams_ShouldReturnBadRequest()
        {
            // Arrange
            var tableName = "Employees";

            var queryString = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "$invalidparam", new StringValues("FirstName eq 'Nancy'") }
            });

            _controller.HttpContext.Request.Query=queryString;

            // Act
            var result = await _controller.QueryAsync(tableName);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid parameters in query string: $invalidparam."));
        }

        [Test]
        public async Task QueryAsync_WithInvalidRightSideParams_ShouldReturnBadRequest()
        {
            // Arrange
            var tableName = "Employees";

            var queryString = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "$filter", new StringValues("FirstName e!q 'Nancy'") }
            });

            _controller.HttpContext.Request.Query=queryString;

            // Act
            var result = await _controller.QueryAsync(tableName);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.That(badRequestResult.Value, Is.EqualTo("Syntax error at position 11 in 'FirstName e!q 'Nancy''."));
        }

        [Test]
        public async Task QueryById_WithValidData_ShouldReturnAResult()
        {
            // Arrange
            var tableName = "Employees";
            var employeeID = 1;
            var expected = new List<object>
            {
                new Dictionary<string, object> {{"EmployeeID", 1}, {"FirstName", "Nancy"}, {"LastName", "Davolio"}, {"Title", "Sales Representative"}, {"BirthDate", new DateTime(1968, 12, 8)}, {"HireDate", new DateTime(1992, 5, 1)}, {"City", "Seattle"}, {"Country", "USA"}, {"TotalOrders", 0}}
            };
            var result = await _controller.QueryByIdAsync(tableName, employeeID.ToString());
            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expected));
        }
        
        [Test]
        public async Task QueryById_WitInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var tableName = "Employees";
            var employeeID = 22;
            var result = await _controller.QueryByIdAsync(tableName, employeeID.ToString());
            // Assert
            var notFoundObjectResult = result as NotFoundObjectResult;
            Assert.NotNull(notFoundObjectResult);
            Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Could not retrieve record with key '{employeeID.ToString()}' from table '{tableName}'."));
        }
        
        [Test]
        public async Task QueryById_WitInvalidIdDataType_ShouldReturnBadRequest()
        {
            // Arrange
            var tableName = "Employees";
            var employeeID = "InvalidID";
            var result = await _controller.QueryByIdAsync(tableName, employeeID);
            // Assert
            var BadRequestObjectResult = result as BadRequestObjectResult;
            Assert.NotNull(BadRequestObjectResult);
            Assert.That(BadRequestObjectResult.Value, Is.EqualTo($"Could not retrieve record with requested key data type '{employeeID}' from table '{tableName}'."));
        }
        
        [Test]
        public async Task QueryById_WitInvalidTable_ShouldReturnNotFound()
        {
            // Arrange
            var tableName = "EasyThere";
            var employeeID = "1";
            var result = await _controller.QueryByIdAsync(tableName, employeeID);
            // Assert
            var notFoundObjectResult = result as NotFoundObjectResult;
            Assert.NotNull(notFoundObjectResult);
            Assert.That(notFoundObjectResult.Value, Is.EqualTo($"Could not retrieve table '{tableName}' with primary key of requested data type."));
        }
    }
}