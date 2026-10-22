namespace FoodDatabase.Tests;

/// <summary>
/// Defines the shared SQL Server fixture and prevents its tests from running concurrently.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DatabaseCollection : ICollectionFixture<SqlServerDatabaseFixture> {
    /// <summary>
    /// The xUnit collection name used by SQL Server integration tests.
    /// </summary>
    public const string Name = "SQL Server database";
}