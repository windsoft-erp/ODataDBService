# ODataDBService

ODataDBService is a .NET Core web API that provides a generic OData interface to a Microsoft SQL Server database. The API supports querying, filtering, sorting, and pagination of data.

## Requirements

- .NET Core 3.1 or later
- Visual Studio 2019 or later
- Docker
- A Microsoft SQL Server database

## Installation

1. Clone the repository to your local machine.
2. Open the solution in Visual Studio.
3. Configure your database connection string in the `appsettings.json` file for each project.
4. Build the solution to restore all required packages and dependencies.

## Projects

### ODataDBService

The `ODataDBService` project is the main project in the solution that provides the OData interface to the Microsoft SQL Server database. It includes the following endpoints:

- `/odata/{entity}`: Gets all records for the specified entity.
- `/odata/{entity}({key})`: Gets a single record for the specified entity by key.
- `/odata/{entity}?$filter={filter}`: Gets all records for the specified entity that match the specified filter.
- `/odata/{entity}?$orderby={orderby}`: Gets all records for the specified entity sorted by the specified order.
- `/odata/{entity}?$skip={skip}&$top={top}`: Gets a subset of the records for the specified entity based on the specified skip and top values.
- `/api/SQLCommand`: Executes SQL commands directly against the database. The SQL command should be a stored procedure that takes no parameters and returns an integer value.

### ODataDBUp

The `ODataDBUp` project provides a command-line tool for deploying SQL scripts to a Microsoft SQL Server database. It uses the `DbUp` library to deploy database scripts, which are stored in the `Scripts` folder. To run the project, run the console app from Visual Studio and configure your database in the `appsettings.json` file.

### ODataDBTester

The `ODataDBTester` project is a .NET Core test project that includes unit tests for the `ODataDBService` project. The tests are designed to be run from the Visual Studio test runner.

## Deployment with Docker

To deploy the ODataDBService API using Docker, follow these steps:

1. Open a terminal or command prompt.
2. Change the working directory to the root of the project.
3. Run `docker-compose build` to build the Docker images for the `ODataDBService` and `MSSQLServer` services.
4. Run `docker-compose up -d` to start the Docker containers for the `ODataDBService` and `MSSQLServer` services.
5. Access the API at `http://localhost:8080/ODataV4`.

## Acknowledgements

This project uses the following open source libraries:

- OData: https://github.com/OData/WebApi
- Entity Framework Core: https://github.com/dotnet/efcore
- Microsoft.AspNetCore.OData: https://github.com/OData/AspNetCoreOData
- DbUp: https://github.com/DbUp/DbUp
- NUnit: https://github.com/nunit/nunit
- Microsoft.NET.Test.Sdk: https://github.com/microsoft/vstest
