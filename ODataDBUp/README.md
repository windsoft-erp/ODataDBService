# ODataDBUp

ODataDBUp is a .NET Core console application that uses the DbUp library to deploy database scripts to a Microsoft SQL Server database. The application reads SQL scripts from the \Scripts folder and deploys them to the configured database.

## Requirements

- .NET Core 3.1 or later
- A Microsoft SQL Server database

## Installation

1. Clone the repository to your local machine.
2. Open the project in Visual Studio.
3. Configure your database connection string in the `appsettings.json` file.
4. Add any new SQL scripts to the \Scripts folder. Each new script must be added as an embedded resource so that Visual Studio can see it.

## Usage

To run the application, open the command prompt or terminal and navigate to the ODataDBUp project directory. Then run the following command:
dotnet run

This will start the console application and display a menu with three options:

1. **Drop database and deploy**: This option drops the existing database and deploys all of the SQL scripts in the \Scripts folder to a new database.

2. **Deploy**: This option deploys all of the SQL scripts in the \Scripts folder that have not been previously deployed to the database.

3. **Exit**: This option exits the application.

Select the appropriate option from the menu to deploy the SQL scripts to your database.

## Configuration

The ODataDBUp project uses the `appsettings.json` configuration file to specify the database connection string used by the tests. To configure the database connection string, open the `appsettings.json` file and modify the `"DefaultConnection"` property to point to your own database.

```json
{
  "ConnectionStrings": {
    "Sql": "Server=localhost;Database=ODataDBService;User Id=sa;Password=1234;"
  }
}
```

## Acknowledgements

This project uses the following open source libraries:

- DbUp: https://github.com/DbUp/DbUp
