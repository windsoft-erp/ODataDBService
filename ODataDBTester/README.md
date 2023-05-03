# ODataDBTester

ODataDBTester is a .NET Core test project that includes unit tests for the OData DB service. The tests are designed to be run from the Visual Studio test runner.

## Requirements

- .NET Core 3.1 or later
- Visual Studio 2019 or later

## Installation

1. Clone the repository to your local machine.
2. Open the project in Visual Studio.
3. Build the solution to restore all required packages and dependencies.

## Usage

To run the tests, open the Test Explorer window in Visual Studio (Test > Test Explorer). Then select the tests you want to run and click the "Run" button. 

The tests in the ODataDBTester project cover various aspects of the OData DB service, such as testing the behavior of the OData endpoints, verifying that the service responds correctly to HTTP requests, and checking that the database is updated correctly when new data is added or modified.

## Configuration

The ODataDBTester project uses the `appsettings.json` configuration file to specify the database connection string used by the tests. To configure the database connection string, open the `appsettings.json` file and modify the `"DefaultConnection"` property to point to your own database.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ODataDBService;User Id=sa;Password=1234;"
  }
}
```

## Acknowledgements

This project uses the following open source libraries:

NUnit: https://github.com/nunit/nunit
Microsoft.NET.Test.Sdk: https://github.com/microsoft/vstest

