using DbUp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Dapper;

string connectionString = GetConnectionString();
if (connectionString == null)
{
    Console.WriteLine("Connection string not found in configuration.");
    return;
}

Console.WriteLine("1. Deploy scripts to existing database.");
Console.WriteLine("2. Drop the database and deploy scripts.");

Console.WriteLine("Enter '1' or '2': ");
var key = Console.ReadKey();
Console.WriteLine();

switch (key.KeyChar)
{
    case '1':
        CreateDatabaseIfNotExists(connectionString);
        DeployScripts(connectionString);
        break;
    case '2':
        Console.WriteLine("WARNING: This will permanently delete the database and all data it contains.");
        Console.WriteLine("Are you sure you want to continue? Enter 'yes' to confirm, or any other key to cancel: ");
        var confirm = Console.ReadLine();

        if (confirm.ToLower() == "yes")
        {
            DropDatabase(connectionString);
            CreateDatabaseIfNotExists(connectionString);
            DeployScripts(connectionString);
        }
        else
        {
            Console.WriteLine("Operation cancelled.");
        }
        break;
    default:
        Console.WriteLine("Invalid input.");
        break;
}

static string GetConnectionString()
{
    var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    return config?.GetConnectionString("Sql");
}

static void CreateDatabaseIfNotExists(string connectionString) => EnsureDatabase.For.SqlDatabase(connectionString);

static void DropDatabase(string connectionString)
{
    var builder = new SqlConnectionStringBuilder(connectionString);
    var databaseName = builder.InitialCatalog;

    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        connection.Execute($@"IF EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}') DROP DATABASE [{databaseName}]");
    }
}

static void DeployScripts(string connectionString)
{
    var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .LogToConsole()
    .Build();

    var result = upgrader.PerformUpgrade();

    if (!result.Successful)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(result.Error);
        Console.ResetColor();
#if DEBUG
        Console.ReadLine();
#endif
        Environment.Exit(-1);
    }
}