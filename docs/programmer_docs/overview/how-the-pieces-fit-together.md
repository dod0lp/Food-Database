[Programmer documentation](../README.md)

# How the pieces fit together

```text
Angular UI
  -> /api/... HTTP requests
ASP.NET Core API
  -> controllers + contracts + authentication
Shared C# domain model <-> Mapper <-> EF Core entities
DB_FoodContext
  -> SQL Server tables

Main.cs (console test program)
  -> Food repository / DB_FoodContext
    -> same SQL Server tables
```

`Food-Database.csproj` intentionally excludes `Api/**/*.cs`. The API is a separate Web SDK project referencing it, which keeps the web host out of the console executable -- can run in the background by docker.
