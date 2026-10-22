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
- `backend-cli` is the interactive console program for browsing and managing foods.
- `backend-maintenance` runs database seeding and data checks without the
  interactive CLI.
- `backend-http` is the ASP.NET Core HTTP API used by the web application.
- `backend-tests` contains automatic SQL Server integration tests. Each run creates
  a new empty temporary database and drops it afterward.

### Run backend tests
- Start the local SQL Server service first:
    ```sh
    docker compose up -d sqlserver
    ```
- Run the tests:
    ```sh
    dotnet test backend-tests/FoodDatabase.Tests.csproj
    ```
- Set `KEEP_FOOD_DATABASE_TEST_DATABASES=1` to keep the generated database for
  investigation. The test database name starts with `FoodDatabaseTests_`.

### Database maintenance
Start SQL Server, then run either maintenance command explicitly:

```sh
docker compose up -d sqlserver
docker compose --profile maintenance run --rm maintenance --seed
docker compose --profile maintenance run --rm maintenance --checkdb
```

The check report is written to
`backend-maintenance/reports/database-constraints-check.txt`.