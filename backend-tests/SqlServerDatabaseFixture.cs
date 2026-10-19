using DotNetEnv;
using Food_Database.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FoodDatabase.Tests;

public sealed class SqlServerDatabaseFixture : IAsyncLifetime {
    private const string KeepDatabasesVariable = "KEEP_FOOD_DATABASE_TEST_DATABASES";

    private readonly string _databaseName = $"FoodDatabaseTests_{Guid.NewGuid():N}";
    private readonly string _server;
    private readonly string _saPassword;
    private bool _databaseCreated;

    public SqlServerDatabaseFixture() {
        LoadRepositoryEnvironment();
        _server = GetSetting("FOOD_DATABASE_TEST_SERVER")
            ?? $"127.0.0.1,{GetSetting("SQL_PORT") ?? "1433"}";
        _saPassword = GetSetting("FOOD_DATABASE_TEST_SA_PASSWORD")
            ?? GetSetting("SA_PASSWORD")
            ?? throw new InvalidOperationException(
                "Set SA_PASSWORD in .env or FOOD_DATABASE_TEST_SA_PASSWORD.");
    }

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

    public DB_FoodContext CreateContext() {
        DbContextOptions<DB_FoodContext> options =
            new DbContextOptionsBuilder<DB_FoodContext>()
                .UseSqlServer(CreateTestConnectionString())
                .Options;

        return new DB_FoodContext(options);
    }

    private string CreateAdminConnectionString() =>
        new SqlConnectionStringBuilder {
            DataSource = _server,
            InitialCatalog = "master",
            UserID = "sa",
            Password = _saPassword,
            Encrypt = true,
            TrustServerCertificate = true
        }.ConnectionString;

    private string CreateTestConnectionString() =>
        new SqlConnectionStringBuilder {
            DataSource = _server,
            InitialCatalog = _databaseName,
            UserID = "sa",
            Password = _saPassword,
            Encrypt = true,
            TrustServerCertificate = true
        }.ConnectionString;

    private static string QuoteIdentifier(string identifier) =>
        $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";

    private static string? GetSetting(string name) =>
        Environment.GetEnvironmentVariable(name);

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