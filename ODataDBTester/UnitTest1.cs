using Moq;
using ODataDBService.Controllers;
using ODataDBService.Models;
using ODataDBService.Services;
using ODataDBService.Services.Repositories;
using Microsoft.AspNetCore.Mvc;
using DynamicODataToSQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ODataV4Tests
{
    [TestFixture]
    public class ODataV4QueryTests
    {
        private Mock<IODataV4Repository> _mockODataV4Repository;
        private Mock<IODataToSqlConverter> _mockODataToSqlConverter;
        private ODataV4Repository _oDataV4Repository;
        private ODataV4Service _oDataV4Service;
        private ODataV4Controller _controller;

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _mockODataToSqlConverter = new Mock<IODataToSqlConverter>();
            _oDataV4Repository = new ODataV4Repository(config, _mockODataToSqlConverter.Object);
            _oDataV4Service = new ODataV4Service(_oDataV4Repository);
            var logger = Mock.Of<ILogger<ODataV4Controller>>();
            _controller = new ODataV4Controller(logger, _oDataV4Service);
            _mockODataV4Repository = new Mock<IODataV4Repository>();
        }

        [Test]
        public async Task QueryAsync_WithValidParameters_ShouldReturnODataQueryResult()
        {
            // Arrange
            var tableName = "TestTable";
            var expected = new ODataQueryResult { Count = 1, Value = new List<object>() };
            _mockODataV4Repository.Setup(x => x.QueryAsync(tableName, null, null, null, 10, 0))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.QueryAsync(tableName);

            // Assert
            var okResult = result as OkObjectResult;
            var actual = okResult.Value as ODataQueryResult;
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            Assert.That(actual.Value, Is.EqualTo(expected.Value));
        }

        [Test]
        public async Task QueryAsync_WithInvalidParameters_ShouldReturnBadRequest()
        {
            // Arrange
            var tableName = "TestTable";
            var expected = new ODataQueryResult { Count = 1, Value = new List<object>() };
            _mockODataV4Repository.Setup(x => x.QueryAsync(tableName, null, null, null, 10, 0))
                .ReturnsAsync(new ODataQueryResult { Count = 0, Value = new List<object>() });
            // Act
            var result = await _controller.QueryAsync(tableName, "$invalidparam");

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}
