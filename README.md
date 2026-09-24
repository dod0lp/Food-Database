# Food-Database
**C#** + **Angular** webapp providing *information about food*, meals and its *nutrients*.

# Getting started

## Prerequisites
### Using app
- **Docker**
  - Will download everything needed to start and use the app.
  - On windows you probably need to have Docker Desktop running.
- **Web browser** to browse application.

### Development
- **dotnet** to seed data or check if nutrient data are within application logic
- Optional for better development:
  - **Visual Studio**
  - *Some environment* for **Angular**
- Development is with following versions [old project version]
  - **.NET** *10.0.10* [8.0]
  - **EntityFramework** *10.0.10* [8.0]
  - **TypeScript** *5.9.3* [5.5]
  - **Angular** is set to *^21.0.0* [18]

## Documentation

- [Programmers documentation](docs/programmers-docs.md)
- [User documentation](docs/user-docs.md)
- [User stories](docs/user%20stories.md)

## Setup
- Following will start `SQL Server`, `.NET API`, and the `Angular` web app:
    ```sh
    docker compose up --build
    ```
    *You need to wait for containers to start.*

## Running application
- **Open** webapp on <http://localhost:4200>.
- Set `WEB_PORT`, `API_PORT`, and `SQL_PORT` in `.env` to
  configure the web, API, and SQL Server ports. Only `WEB_PORT` is published
  to the host; the API and database are reachable only by the other Compose
  services.
- The `.env` is required by Docker Compose.
  Its values are development defaults.\
  **Production deployment must set new values without committing `.env` file.**

## Shut down
- From the repository root, use `docker compose down` to **stop services**.
- The database persists in the `sqlserver_data` docker volume.
- Login data has its own volume `api_data_protection`.
- `docker compose down -v` to remove **ALL** database data, even those logins.

## Development

### Backend projects
- `backend-library` is the shared domain, data-access, and CSV-import library.
- `backend-cli` is the console program for testing, database values checks, seeding...
- `backend-http` is the ASP.NET Core HTTP API used by the web application.

### Seed dummy data (dotnet required)
- Uses `data/seed_data/food.csv` with predefined column names.
- File is from some open-source dataset.
  - This particular file has missing salt values, so they are randomly generated, so it's technically possible that sum of columns is bigger than 100.
- To seed data, tested only for empty database, run
    ```sh
    dotnet run --project backend-cli/FoodDatabase.Cli.csproj -- --seed
    ```

### Check databse values (dotnet required)
- To check database values if they fall within application logic, run
    ```sh
    dotnet run --project backend-cli/FoodDatabase.Cli.csproj -- --checkdb
    ```

#### Potential development issues
When I was writing code on windows, there have been error for clean `git clone` setup, that it couldn't load startup script because of line endings.
I hopefully fixed that.
If you are developing on Windows and `startup.sh` you get error about, first thing to look for is
line endings in `database/startup.sh`.