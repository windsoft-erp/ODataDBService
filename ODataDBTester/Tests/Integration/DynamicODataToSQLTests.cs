using DynamicODataToSQL;
using SqlKata.Compilers;

namespace ODataDBTester.Tests.Integration
{
    [TestFixture]
    public class ODataToSqlConverterTests
    {
        [TestFixture]
        public class SqlConverter
        {
            private EdmModelBuilder _edmModelBuilder;
            private SqlServerCompiler _sqlServerCompiler;
            private ODataToSqlConverter _oDataToSqlConverter;

            [SetUp]
            public void Setup()
            {
                _edmModelBuilder = new EdmModelBuilder();
                _sqlServerCompiler = new SqlServerCompiler { UseLegacyPagination = false };
                _oDataToSqlConverter = new ODataToSqlConverter(_edmModelBuilder, _sqlServerCompiler);
            }

            [Test]
            public void ConvertToSQL_ExpectedBehavior()
            {
                // Arrange
                var tableName = "Products";
                var tryToParseDates = true;
                var odataQuery = new Dictionary<string, string>
                {
                    { "select", "Name, Type" },
                    { "filter", "contains(Name,'Tea')" },
                    { "orderby", "Id desc" },
                    { "top", "20" },
                    { "skip", "5" },
                };
                var count = false;
                var expectedSQL = @"SELECT [Name], [Type] FROM [Products] WHERE [Name] like @p0 ORDER BY [Id] DESC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY";
                var expectedSQLParams = new Dictionary<string, object>
                {
                    { "@p0", "%Tea%" },
                    { "@p1", 5 },
                    { "@p2", 20 },
                };

                // Act
                var result = _oDataToSqlConverter.ConvertToSQL(tableName, odataQuery, count, tryToParseDates);

                // Assert
                var actualSQL = result.Item1;
                var actualSQLParams = result.Item2;

                Assert.That(actualSQL, Is.EqualTo(expectedSQL).IgnoreCase);
                Assert.That(actualSQLParams, Is.EqualTo(expectedSQLParams));
            }
        }
    }
}