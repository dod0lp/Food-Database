# Food Database API

The API is the web-facing layer over the existing `Food-Database` project. It
uses ASP.NET Core Identity with `Users_DBEntity : IdentityUser<int>`, so a
signed-in user has the same integer ID already referenced by favorites, food
options, remarks, and user-created foods.

## First-time database setup

Run the normal SQL initializer from an empty database. It creates `Users` with
the Identity columns already present, then creates the standard Identity support
tables in [`04_init_identity.sql`](../../web/SQL/init-scripts/04_init_identity.sql).

## Run locally

Set either a connection string or the existing SQL Docker variables without
committing them, then start the API:

```powershell
$env:ConnectionStrings__FoodDatabase = 'Server=localhost;Database=db_food;User Id=...;Password=...;TrustServerCertificate=True'
dotnet run --project Api/Food-Database.Api.csproj --urls http://localhost:5080
```

In another terminal, run `npm install` then `npm start` from `src/web/angular`.
The Angular proxy sends `/api` calls to the API. The `src/docker-compose.yml`
API service loads its `DB_*` values from `src/web/docker/.env`.

## Initial endpoints

- `POST /api/auth/register`, `login`, `logout`; `GET /api/auth/me`
- `GET /api/foods`, `GET /api/foods/{id}`
- authenticated: `POST /api/foods/composites`, `GET /api/foods/favorites`,
  `PUT /api/foods/{id}/favorite`

Authentication is an HttpOnly same-site cookie, rather than a token stored in
Angular JavaScript. The API has a strict password policy, lockout after failed
logins, and rate limiting on authentication routes. Terminate TLS at the
deployment proxy and use HTTPS in production.
