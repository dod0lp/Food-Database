[Programmer documentation](../README.md)

# Data integrity and security checklist

- For **authenticated calls never accept an input user ID** as owner.
  - Use `FoodsController.CurrentUserId()`.
- Filter private remarks, options, and favorites by authenticated user ID.
- SQL nutrient gram fields are non-negative. `NULL` for non-existent.
- `Main --checkdb` is just reader function, does't prune or anything.
  - Checks:
    values no greater than 100g, total mass less than than 100g, and that sugar or saturated fat don't exceed their total nutrient.
- SQL constraints **do not represent application logic**. They are just most obvious and basci checks.
- Store only positive composite weights, normalized to 100g.
- Validate database lengths before exposing SQL/EF errors:
  - Constants in: `Food_Database.Database.Descriptors`
    - food `name`: 200 chars
    - food `description` and `remarks`: 4.000 chars
- Current password policy is in [.AddIdentity](../../../src/backend/Api/Program.cs#L40).
