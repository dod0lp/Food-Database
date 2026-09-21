[Programmer documentation](../README.md)

# EF Core layer

`DB_FoodContext` inherits from:

```csharp
IdentityDbContext<Users_DBEntity, IdentityRole<int>, int>
```

It calls `base.OnModelCreating(modelBuilder)` first, then maps food tables and maps the Identity user to the existing `Users` table. Removing the base call breaks Identity's model.

The context exposes `Food`, `FoodIngredients`, `UserCreatedFood`, `UserFoodOptions`, `UserFoodRemark`, and inherited Identity sets including `Users` and `Roles`.

The fresh schema is defined in `infrastructure/database/init-scripts/`, not EF. EF has schema [from SQL](../database/data-model.md).
