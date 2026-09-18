# Food Database Setup

## Setup
- Choose this folder, `src/web/docker`.
- Following will start `SQL Server`, `.NET API`, and the `Angular` web app:
    ```sh
    docker compose up --build
    ```
    *You need to wait for containers to start.*

## Running application
- **Open** webapp on <http://localhost:4200>.

## Shut down
- To **stop services**, use `docker compose down`.
- The database persists in the `sqlserver_data` docker volume.
- `docker compose down -v` to remove database data.

## Development

### Seed dummy data (dotnet required)
- Uses `src/C#/Parser/food.csv` file with predefined column names.
- File is from some open-source dataset.
  - This particular file has missing salt values, so they are randomly generated, so it's technically possible that sum of columns is bigger than 100.
- To seed data, tested only for empty database, run
    ```sh
    dotnet run --project ../../C#/Food-Database.csproj -- --seed
    ```

### Check databse values (dotnet required)
- To check database values if they fall within application logic, run
    ```sh
    dotnet run --project ../../C#/Food-Database.csproj -- --checkdb
    ```

#### Potential development issues
When I was writing code on windows, there have been error for clean `git clone` setup, that it couldn't load startup script because of line endings.
I hopefully fixed that.
If you are developing on windows, and you get error about `startup.sh`, first thing to look for is line endings on `../SQL/startup.sh`.