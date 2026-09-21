[Programmer documentation](../README.md)

# API layer

`src/backend/Api/Program.cs` configures context/repositories, SQL Server connection, Identity, `food-auth` cookie, authorization, CORS, forwarded headers, rate-limiting.\
Connection settings are from `ConnectionStrings:FoodDatabase` from args, or `DB_Descriptors.MakeConnectionString` from .env file.

Identity is integrated in the existing `Users` table stores as `Users_DBEntity : IdentityUser<int>`, using the same integer IDs.

**Controllers** should:
- Validate HTTP input
- gGet current user ID from claims
- Call a repository
- Return HTTP responses

**Repository** has responsibility for:
- Operations with food database entities
- EF queries (which are used by API)

## Current API surface

| Endpoint | Auth | Responsibility |
| --- | --- | --- |
| `POST /api/auth/register`, `login` | No | Register or sign in. |
| `POST /api/auth/logout`, `GET /api/auth/me` | Yes | Sign out or get user. |
| `GET /api/foods` | No | Page system foods. |
| `GET /api/foods/mine` | Yes | Page current user's created foods. |
| `GET /api/foods/{foodId}` | No | Get one food and its ingredients. |
| `POST /api/foods` | Yes | Create simple food. |
| `POST /api/foods/composites` | Yes | Create composite food. |
| `GET/PUT/DELETE /api/foods/{foodId}/favorite` | Yes | Check, set, or remove favorite. |
| `GET /api/foods/favorites` | Yes | List favorite with options and remarks. |
| `DELETE /api/foods/{foodId}/favorite/options/{weight}` | Yes | Remove one option. |

Request/response records are in `src/backend/Api/Contracts/`.
Tried to map names such as `energyKcal` properly, but in `Angular` they are camelCase and in `API C# Contracst` PascalCase.
