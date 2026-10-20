## Potential issues, wrong design, fixes, etc

Some of them are pointed out in documentation.

1. Secrets/configuration handling is unsafe.

   .env is committed, despite the README saying production values should not be committed. Also, DatabaseDescriptors.cs (/C:/Users/Dominik/Desktop/temp/Food-Database/backend-library/Database/DatabaseDescriptors.cs:33) contains a fallback database password. Remove secrets from Git history, rotate them, add .env to .gitignore, commit an .env.example, and fail startup when required production configuration is absent.
2. There are no automated tests or CI.

   I found no .NET test project, Angular specs, or pipeline configuration. This is the main reason I would not call it robust yet. Add unit tests for nutrient scaling/mapping, integration tests for API/auth/database behavior, and a small end-to-end UI suite. Then run build/test/lint in CI.
3. Writes are not atomic.

   Creating food, adding ingredients, and marking it as favorite occur through multiple SaveChangesAsync calls and separate controller calls. A failure can leave partially-created records. For example, FoodsController.cs (/C:/Users/Dominik/Desktop/temp/Food-Database/backend-http/Controllers/FoodsController.cs:157) creates food and favorites it as distinct operations, while Updaters.cs (/C:/Users/Dominik/Desktop/temp/Food-Database/backend-library/Database/Repositories/Food/Updaters.cs:36) saves intermediate state. Wrap complete use cases in explicit transactions.
4. The data layer is becoming a “god repository.”

   The partial Repository mixes querying, persistence, user favorites, ingredient graph construction, validation, and mapping. It works at this scale, but will get hard to change. Introduce application/use-case services such as CreateFoodService, FavoritesService, and FoodQueryService; keep repository methods narrow.
5. Schema evolution is fragile.

   The system maintains hand-written SQL initialization scripts and EF Core mappings, but has no EF migrations or other versioned migration mechanism. This invites schema drift and makes upgrades harder. Adopt one authoritative migration path.
6. Nested composite foods may not return their full ingredient tree.

   Queries load only one ingredient level (Include(...).ThenInclude(...)), while mapping recursively walks children. With EF lazy-loading absent, deeper composite layers do not appear loaded. The nutrition total may still be correct because it is persisted, but the returned ingredient tree can be incomplete.
7. Validation is uneven.

   The API checks some pagination and option constraints, but request DTOs have no declarative validation for field lengths, ingredient count, maximum values, or all nutrient inputs. Oversized strings or malformed input can become database exceptions and generic errors rather than good 400 responses.

A couple of smaller issues:
- Offset pagination is fine now, but may become slow for very large food tables.
- Returning domain models directly from controllers couples the external API to internal domain classes; separate response DTOs would make future changes safer.
- The existing security choices are good, but a real public deployment should use HTTPS-only cookies and carefully review proxy/forwarded-header trust.
- The code is readable overall, although naming and comments are inconsistent in places.

my todo list i found during development:

TEST IT MORE
See food detail what it does for null

Check if user input is more saturated than total fat, and sugars.

Check iser input string for length (db truncates it, but for user feedback)

Display url as name of food, not id?

Better table for nutrients (kinda worked on it already)

Allow users to edit nutrients of food they created (usually you have whole nutrient label, so idk)

Make some common table (component) for browsed food and favorite food nutrients

Page doesn't exist landing page (noe routes to default /)

Spacing favorites.comp.html, app.routes.ts

If user creates food and deletes it from favorites it is in his created food forever (maybe some pruning once in a while, some limit, or option to delete your own foods from db completely)
--conflicts with point below--
User can check any food with id, even other user created (but since there are no remarks visible, is it issue?)
-------
Add option to share food? When nothing references...

Composite food for non-existent nutrient base values just calculates as it was 0. Add some disclaimer.

Add to docs:
How much nutrients from each ingredient
Even user can add description

DatabaseDescriptors use some config folder, add it to docs that this should be production set, and add to gitignore, if easy to migrate docker sourcing from it..

## TODO slopgpt review (2026-10-21)

### Implement soon

- **Backend validation:** enforce name and description lengths, non-negative nutrients, `saturated <= total fat`, and `sugar <= total carbohydrates`. The database has length limits, but the API currently accepts invalid values until SQL rejects them.
- **Unknown nutrients in composites:** a composite must not present a calculated nutrient total when an ingredient's value is unknown. Show it as unknown, with a short UI explanation.
- **Transactions for complete operations:** creating food and adding it to favorites currently writes separately. A failed second write can leave a partially-created food.
- **Schema migrations:** useful before changing the persistent Docker database schema. The current scripts plus EF mappings can drift.
- **Configuration cleanup:** `.env` is ignored, but the legacy CLI and maintenance configuration still has fallback credentials in `DatabaseDescriptors.cs`. Remove those defaults, use one environment-variable path, and add an `.env.example`.
- **Tests and CI:** the no-tests item is stale: there are now 11 SQL integration tests. CI is still worth adding later.
- **`GetFoodForUserAsync` TODO:** this is a real bug. A user can have multiple package options, but the method calls `SingleOrDefault`, so it can throw. It should accept a chosen weight, or use 100 g when no explicit weight is requested; it should not silently choose one option.

### Already done

- The nested composite tree issue is fixed by recursive ingredient loading and its tests.
- A food-detail not-found state exists. Unknown routes redirect to `/`.

### Product decisions needed before implementation

- **Deleting user-created food:** decide whether user-created foods are public. They are currently readable by ID. If they remain public, deletion should be creator-only and reject deletion while the food is referenced by composites or other users' favorites.
- **Editing nutrients of created food:** useful after deletion and ownership rules exist. It needs creator-only authorization and the same validation above.
- **Sharing:** unnecessary if created foods remain public. Add a visibility/share model only if foods should be private by default.

### Low priority or cosmetic

- Separate generic 404 page.
- Friendly URL slugs instead of IDs: keep IDs; add optional slugs only for nicer links.
- Shared nutrient-table component: sensible cleanup, but common nutrient-row helpers already reduce duplication.
- Favorites spacing and table styling: do during a UI pass.
- Documentation on ingredient nutrient contributions and descriptions: worthwhile after the behavior is finalized.