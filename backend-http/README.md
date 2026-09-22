# Food Database API

The API is the HTTP layer using the shared `backend-library` project operations with Entity Framework.
It uses ASP.NET Core Identity with `Users_DBEntity : IdentityUser<int>`, so a
signed-in user has the same integer ID already used for food data.

## Run locally

Connection-string values are read from the repository-root `.env`. Run Docker Compose
from the repository root as described in the main README.

## Endpoints

- `POST /api/auth/register`, `login`, `logout`;
- `GET /api/auth/me`
- `GET /api/foods`, `GET /api/foods/{id}`
- **authenticated**:
  - `POST /api/foods/composites`, `GET /api/foods/favorites`,
  `PUT /api/foods/{id}/favorite`

## Auth

Authentication is an HttpOnly same-site cookie (not token stored in Angular JS).
The API has set password policy, lockout after failed logins, etc...
Reverse proxy handle HTTPS/TLS, while ASP.NET app itself can communicate over plain HTTP behind that proxy.
