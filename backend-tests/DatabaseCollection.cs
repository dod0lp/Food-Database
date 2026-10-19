namespace FoodDatabase.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DatabaseCollection : ICollectionFixture<SqlServerDatabaseFixture> {
    public const string Name = "SQL Server database";
}