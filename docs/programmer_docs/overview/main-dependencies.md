[Programmer documentation](../README.md)

# Main dependencies

| Dependency | Used for |
| --- | --- |
| `.NET 10` | All C# projects target `net10.0`. |
| `Entity Framework Core` | Work with database (query, save, update,...). |
| `AspNetCore.Identity` | Users, passwords, cookie login, lockout, and Identity schema. |
| `CsvHelper` | Seed CSV import. |
| `DotNetEnv` | Console project's `config/app.env` database settings. |
| `SQL Server` | Database duh. |
| `ASP.NET Core` | HTTP API, DI, CORS, cookie auth, authorization, rate limiting. |
| `Angular 21` | Browser UI (and sending API requests). |
| `Docker` | Starts and downloads everything with one command |

Exact C# package versions are in `src/C#/Food-Database.csproj`.\
API obtains its framework dependencies from the .NET Web SDK.
