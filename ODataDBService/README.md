# ODataV4 API for ASP.NET Core

An ASP.NET Core API project that provides a simple ODataV4 service using MSSQL Server as the database. The API supports querying, inserting, updating, and deleting records in the database, and utilizes Docker for deployment.

## Prerequisites

- Visual Studio
- Docker
- MSSQL Server
- .NET Core 6 https://dotnet.microsoft.com/en-us/download/dotnet/6.0

## Getting Started

1. Clone the repository to your local machine.
2. Open the solution in Visual Studio.
3. Update the `appsettings.json` file with your MSSQL Server connection string.
4. Build and run the application.

## API Endpoints

- `GET /ODataV4/{tableName}`: Query records from a table.
  - Query parameters: `$select`, `$filter`, `$orderby`, `$top`, `$skip`
- `GET /ODataV4/{tableName}({key})`: Query records from a table.
- `DELETE /ODataV4/{tableName}({key})`: Delete a record from a table using its key.
- `POST /ODataV4/{tableName}`: Insert a new record into a table.
- `PUT /ODataV4/{tableName}({key})`: Update an existing record in a table using its key.
- `DELETE /ODataV4/invalidate-cache/{tableName}`: Invalidate the table info cache for a specific table.
- `POST /SQLCommand/{storedProcedureName}`: Execute SQL stored procedure directly against the database

## Configuration

The ODataDBService project uses the `appsettings.json` configuration file to specify the database connection string used by the tests. To configure the database connection string, open the `appsettings.json` file and modify the `"DefaultConnection"` property to point to your own database.

```json
{
  "ConnectionStrings": {
    "Sql": "Server=localhost;Database=ODataDBService;User Id=sa;Password=1234;"
  }
}
```

## Deployment with Docker

1. Open a terminal or command prompt.
2. Change the working directory to the root of the project.
3. Run `docker build -t odatadbapi .` to build the Docker image.
4. Run `docker run -d -p 8080:80 --name odatadbapi odatadbapi` to start a container with the built image.
5. Access the API at `http://localhost:8080`.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
