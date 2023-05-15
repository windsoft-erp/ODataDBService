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

if (args.Length > 0)
{
    if (int.TryParse(args[0], out int option))
    {
        switch (option)
        {
            case 1:
                ExecuteOperation1();
                break;
            case 2:
                ExecuteOperation2("yes");
                break;
            default:
                Console.WriteLine("Invalid argument. Available options: 1, 2");
                break;
        }
    }
    else
    {
        Console.WriteLine("Invalid argument. Please provide a valid option: 1 or 2");
    }
}
else
{
    RunStandardMode();
}

void RunStandardMode()
{
    while (true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Database Management ===");
        Console.ResetColor();
        Console.WriteLine("1. Deploy scripts to existing database.");
        Console.WriteLine("2. Drop the database and deploy scripts.");
        Console.WriteLine("3. Exit");
        Console.WriteLine();
        Console.Write("Enter the option number: ");

        if (!int.TryParse(Console.ReadLine(), out int option))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Invalid input. Press any key to try again.");
            Console.ResetColor();
            Console.ReadKey();
            continue;
        }

        if (option == 3)
        {
            break;
        }

        try
        {
            switch (option)
            {
                case 1:
                    ExecuteOperation1();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Operation completed successfully. Press any key to continue.");
                    Console.ResetColor();
                    Console.ReadKey();
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Red;
                    ExecuteOperation2();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Operation completed successfully. Press any key to continue.");
                    Console.ResetColor();
                    Console.ReadKey();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Invalid input. Press any key to try again.");
                    Console.ResetColor();
                    Console.ReadKey();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}

void ExecuteOperation1()
{
    CreateDatabaseIfNotExists(connectionString);
    DeployScripts(connectionString);
}

void ExecuteOperation2(string? confirmed = null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("WARNING: This will permanently delete the database and all data it contains.");
    Console.ResetColor();
    Console.Write("Are you sure you want to continue? Enter 'yes' to confirm: ");
    var confirm = confirmed ?? Console.ReadLine();

    if (confirm.ToLower() == "yes")
    {
        DropDatabase(connectionString);
        CreateDatabaseIfNotExists(connectionString);
        DeployScripts(connectionString);
    }
    else
    {
        Console.WriteLine("Operation cancelled. Press any key to continue.");
        Console.ReadKey();
    }
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
    string masterConnectionString = connectionString.Replace(databaseName, "master");

    using (var connection = new SqlConnection(masterConnectionString))
    {
        connection.Open();

        // Get the list of active sessions for the database
        var sessions = connection.Query<int>($"SELECT session_id FROM sys.dm_exec_sessions WHERE database_id = DB_ID('{databaseName}')");

        // Kill the active sessions
        foreach (var sessionId in sessions)
        {
            connection.Execute($"KILL {sessionId}");
        }

        // Drop the database
        connection.Execute($"IF EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}') DROP DATABASE [{databaseName}]");
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