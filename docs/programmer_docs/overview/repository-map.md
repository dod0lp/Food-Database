[Programmer documentation](../README.md)

# Repository map

| Location | Responsibility/Content |
| --- | --- |
| `src/backend/Food-Database.csproj` | Shared project: domain types, EF Core models, repositories, CSV parser. `Main()` tests. |
| `src/backend/Main.cs` | Console test, plus --arg for setup, etc. |
| `src/backend/Food/` | Domain `Food`, `Nutrients`, and nutrient value types. |
| `src/backend/Database/Models/` | EF Core entities and `DB_FoodContext`. |
| `src/backend/Database/Repositories/Food/` | Food data-access and business operations; `Repository` class to work with repo. |
| `src/backend/Database/Repositories/Auth/` | Wrapper around ASP.NET Core Identity `UserManager`. |
| `src/backend/Api/` | ASP.NET Core Web API: startup, contracts, and controllers. |
| `src/backend/Parser/` | CSV import used by `Main() --seed`. |
| `infrastructure/database/` | SQL Server Docker image and fresh-database creation scripts. |
| `infrastructure/compose.yaml` | Docker Compose setup for SQL Server, API, and Angular. |
| `src/web/angular/` | Angular UI and HTTP. |
| `docs/user stories.txt` | What user can do. |
