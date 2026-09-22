[Programmer documentation](../README.md)

# Food repository

`Food_Database.Database.Repositories.Foods.Repository` is a scoped service around one `DB_FoodContext`.

| File | Purpose |
| --- | --- |
| `Base.cs` | Context injection and `SaveChangesDBAsync`. |
| `Getters.cs` | Reads, most use `AsNoTracking`. |
| `Updaters.cs` | Creation/updates of objects. |
| `Helpers.cs` | Existence, ownership, ingredient checks, etc. |
| `Mapper.cs` | Entity/domain conversion and normalization. |

## Useful operations

- `GetFoodAsync`, `GetFoodsAsync`, `GetSystemFoodsAsync`, and `GetUserCreatedFoodsAsync` read foods.
- `GetFavoritesWithOptionsAsync` returns current-user remarks and sorted options.
- `AddFoodAsync` persists a food and creates `UserCreatedFood` when set `userId > 0`.
- `CreateCompositeFoodAsync` calculates a composite and saves it in database.
  - Also saves all non-existent ingredients, if they are not already saved.
- `SetFavoriteFoodAsync` adds favourite status and can update remarks/options.
  - Option and favourite remove/update methods manage user-specific data.

## Save behaviour is not uniform

Functions that also save database have it written in remarks. Otherwise functions shouldn't save content.\
To save, you must call `SaveChangesDBAsync`. `Main.TestFavoriteFoodOptions` shows that pattern.\
**Decide and document** the save for every new method (convetion: *save is not used implicitly* [unless needed]).

## Composite-food

### Example how it works

Composite food can even be created out of other composite foods.

For `A: xxx g` and `B: xyz g`:
1. `CreateFoodFromExistingAsync` loads valid existing ingredients, if they have ID they are queried from database.
2. Each is scaled from 100g normalized weight to requested grams.
3. Nutrients and total weight are summed into a domain `Food`.
4. `AddFoodAsync`normalizes nutrients back to 100g for SQL. With boolean to add ingredients:
   - `SetFoodIngredientsAsync` stores each ingredient amount out of 100g.

Ideally use only positive weights (meaning it exists), and existing IDs. If none are valid the method returns `null`.
Simple check: `Main --checkdb` reports nutrient values sums not equal to 100g, composite foods can easily break that if normalization doesn't work.
