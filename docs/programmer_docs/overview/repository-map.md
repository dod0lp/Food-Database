[Programmer documentation](../README.md)

# Repository map

| Location | Responsibility/Content |
| --- | --- |
| `src/C#/Food-Database.csproj` | Shared project: domain types, EF Core models, repositories, CSV parser. `Main()` tests. |
| `src/C#/Main.cs` | Console test, plus --arg for setup, etc. |
| `src/C#/Food/` | Domain `Food`, `Nutrients`, and nutrient value types. |
| `src/C#/Database/Models/` | EF Core entities and `DB_FoodContext`. |
| `src/C#/Database/Repositories/Food/` | Food data-access and business operations; `Repository` class to work with repo. |
| `src/C#/Database/Repositories/Auth/` | Wrapper around ASP.NET Core Identity `UserManager`. |
| `src/C#/Api/` | ASP.NET Core Web API: startup, contracts, and controllers. |
| `src/C#/Parser/` | CSV import used by `Main() --seed`. |
| `src/web/SQL/` | SQL Server Docker image and fresh-database creation scripts. |
| `src/web/docker/` | Docker Compose setup for SQL Server, API, and Angular. |
| `src/web/angular/` | Angular UI and HTTP. |
| `docs/user stories.txt` | What user can do. |
