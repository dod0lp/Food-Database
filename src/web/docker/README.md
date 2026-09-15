# Food Database Docker stack

From this folder, this docker will start `SQL Server`, `.NET API`, and the `Angular` web app:

```sh
docker compose up --build
```

(You need to wait for containers to start)

To seed data, tested only for empty database, run
```sh
dotnet run --project ../../C#/Food-Database.csproj -- --seed
```

To check database values if they fall within application logic, run
```sh
dotnet run --project ../../C#/Food-Database.csproj -- --checkdb
```

**Open** webapp on <http://localhost:4200>.

**Stop services** with `docker compose down`.
The database persists in the `sqlserver_data` docker volume.
(`docker compose down -v` to remove database data.)