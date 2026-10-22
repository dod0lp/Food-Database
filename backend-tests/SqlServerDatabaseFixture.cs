using DotNetEnv;
using Food_Database.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FoodDatabase.Tests;

/// <summary>
/// Creates one temporary SQL Server database for the test collection and removes it after the run.
/// </summary>
public sealed class SqlServerDatabaseFixture : IAsyncLifetime {
    private const string KeepDatabasesVariable = "KEEP_FOOD_DATABASE_TEST_DATABASES";

    private readonly string _databaseName = $"FoodDatabaseTests_{Guid.NewGuid():N}";
    private readonly string _server;
    private readonly string _saPassword;
    private bool _databaseCreated;

    /// <summary>
    /// Loads SQL Server settings from the repository environment.
    /// </summary>
    public SqlServerDatabaseFixture() {
        LoadRepositoryEnvironment();
        _server = GetSetting("FOOD_DATABASE_TEST_SERVER")
            ?? $"127.0.0.1,{GetSetting("SQL_PORT") ?? "1433"}";
        _saPassword = GetSetting("FOOD_DATABASE_TEST_SA_PASSWORD")
            ?? GetSetting("SA_PASSWORD")
            ?? throw new InvalidOperationException(
                "Set SA_PASSWORD in .env or FOOD_DATABASE_TEST_SA_PASSWORD.");
    }

    /// <summary>
    /// Creates the temporary database and initializes its EF Core schema.
    /// </summary>
    public async Task InitializeAsync() {
        await using SqlConnection connection = new(CreateAdminConnectionString());
        await connection.OpenAsync();

        await using SqlCommand createDatabase = connection.CreateCommand();
        createDatabase.CommandText = $"CREATE DATABASE {QuoteIdentifier(_databaseName)};";
        await createDatabase.ExecuteNonQueryAsync();
        _databaseCreated = true;

        await using DB_FoodContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Drops the temporary database unless it was requested to be retained.
    /// </summary>
    public async Task DisposeAsync() {
        if (!_databaseCreated || string.Equals(
            GetSetting(KeepDatabasesVariable), "1", StringComparison.Ordinal)) {
            return;
        }

        await using SqlConnection connection = new(CreateAdminConnectionString());
        await connection.OpenAsync();

        await using SqlCommand dropDatabase = connection.CreateCommand();
        dropDatabase.CommandText = $"""
            IF DB_ID(N'{_databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE {QuoteIdentifier(_databaseName)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE {QuoteIdentifier(_databaseName)};
            END
            """;
        await dropDatabase.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Creates a context connected to this fixture's temporary database.
    /// </summary>
    public DB_FoodContext CreateContext() {
        DbContextOptions<DB_FoodContext> options =
            new DbContextOptionsBuilder<DB_FoodContext>()
                .UseSqlServer(CreateTestConnectionString())
                .Options;

        return new DB_FoodContext(options);
    }

    /// <summary>
    /// Removes test data while retaining the initialized database schema.
    /// </summary>
    public async Task ResetAsync() {
        await using DB_FoodContext context = CreateContext();
        await context.UserFoodOptions.ExecuteDeleteAsync();
        await context.UserFoodRemark.ExecuteDeleteAsync();
        await context.UserCreatedFood.ExecuteDeleteAsync();
        await context.FoodIngredients.ExecuteDeleteAsync();
        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM [UserFoodFavorites];");
        await context.Food.ExecuteDeleteAsync();
        await context.Users.ExecuteDeleteAsync();
    }

    /// <summary>
    /// Creates the administrator connection used to create and drop databases.
    /// </summary>
    private string CreateAdminConnectionString() =>
        new SqlConnectionStringBuilder {
            DataSource = _server,
            InitialCatalog = "master",
            UserID = "sa",
            Password = _saPassword,
            Encrypt = true,
            TrustServerCertificate = true
        }.ConnectionString;

    /// <summary>
    /// Creates the application connection for the temporary test database.
    /// </summary>
    private string CreateTestConnectionString() =>
        new SqlConnectionStringBuilder {
            DataSource = _server,
            InitialCatalog = _databaseName,
            UserID = "sa",
            Password = _saPassword,
            Encrypt = true,
            TrustServerCertificate = true
        }.ConnectionString;

    /// <summary>
    /// Escapes a SQL Server identifier for generated administration SQL.
    /// </summary>
    private static string QuoteIdentifier(string identifier) =>
        $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";

    /// <summary>
    /// Reads one environment setting.
    /// </summary>
    private static string? GetSetting(string name) =>
        Environment.GetEnvironmentVariable(name);

    /// <summary>
    /// Finds and loads the repository's <c>.env</c> file from the build output path.
    /// </summary>
    private static void LoadRepositoryEnvironment() {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null) {
            string envFile = Path.Combine(directory.FullName, ".env");
            if (File.Exists(envFile)) {
                Env.Load(envFile);
                return;
            }

            directory = directory.Parent;
        }
    }
}