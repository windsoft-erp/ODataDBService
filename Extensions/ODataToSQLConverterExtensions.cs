using Dapper;
using DynamicODataToSQL;
using System.Text.Json;
using System.Text;
using System.Data.SqlClient;
using ODataDBService.Models;

namespace ODataDBService.Extensions
{
    public static class ODataToSQLConverterExtensions
    {
        public static (string, DynamicParameters) ConvertToSQLDelete(this IODataToSqlConverter oDataToSqlConverter, string tableName, string key, string connectionString)
        {
            var tableInfo = GetTableInfo(connectionString, tableName);

            var sql = $"DELETE FROM {tableInfo.TableName} WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}";
            var parameters = new DynamicParameters();
            parameters.Add(tableInfo.PrimaryKey, key);

            return (sql, parameters);
        }

        public static (string, DynamicParameters) ConvertToSQLInsert(this IODataToSqlConverter oDataToSqlConverter, string tableName, Dictionary<string, JsonElement> properties)
        {
            var columnNames = string.Join(", ", properties.Keys);
            var valueParams = string.Join(", ", properties.Keys.Select(key => $"@{key}"));

            var sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({valueParams})";
            var parameters = new DynamicParameters();

            foreach (var (key, value) in properties)
            {
                parameters.Add(key, value.ToObject());
            }

            return (sql, parameters);
        }

        public static (string, DynamicParameters) ConvertToSQLUpdate(this IODataToSqlConverter oDataToSqlConverter, string tableName, string key, Dictionary<string, JsonElement> properties, string connectionString)
        {
            var tableInfo = GetTableInfo(connectionString, tableName);

            var queryParams = new DynamicParameters();

            var sb = new StringBuilder($"UPDATE {tableInfo.TableName} SET ");
            foreach (var property in properties)
            {
                var propertyName = property.Key;
                var propertyValue = property.Value;

                if (propertyName.Equals(tableInfo.PrimaryKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (tableInfo.ColumnNames?.Contains(propertyName, StringComparer.OrdinalIgnoreCase) != true)
                {
                    throw new ArgumentException($"Column {propertyName} does not exist in table {tableName}");
                }

                sb.Append($"{propertyName} = @{propertyName}, ");
                queryParams.Add(propertyName, propertyValue.ToObject());
            }

            sb.Remove(sb.Length - 2, 2); // remove last comma and space
            sb.Append($" WHERE {tableInfo.PrimaryKey} = @{tableInfo.PrimaryKey}");
            queryParams.Add(tableInfo.PrimaryKey, key);

            return (sb.ToString(), queryParams);
        }

        private static TableInfo GetTableInfo(string connectionString, string tableName)
        {
            using var connection = new SqlConnection(connectionString);

            var tableInfo = new TableInfo
            {
                TableName = tableName,
                PrimaryKey = connection.QueryFirstOrDefault<string>(
                    @"SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE TABLE_NAME = @TableName 
            AND CONSTRAINT_NAME LIKE 'PK%'",
                    new { TableName = tableName })
            };

            if (tableInfo.PrimaryKey == null)
            {
                throw new ArgumentException($"Could not find primary key for table {tableName}");
            }

            tableInfo.ColumnNames = connection.Query<string>(
                @"SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = @TableName",
                new { TableName = tableName });

            return tableInfo;
        }
    }
}
