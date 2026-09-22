[Programmer documentation](../README.md)

# Domain model and mapping

`FoodBase.Food` is the in-memory model: *ID, name, weight, nutrients, description, and ingredients*.\
It and `Nutrients` support arithmetic for scaling and combining foods.

`Database.Repositories.Foods.Mapper` is used to map between `Food` and `Food_DBEntity`:
- `ToDomain` maps a row (Food_DBEntity), can scale nutrients for requested weight.
- `ToDomainWithIngredients` recursively maps loaded ingredient relations if boolean there is set.
- `MapToEntityNormalized` normalizes entity weight and nutrient values.
- `NormalizeFood` and `NormalizeNutrients` implement the normalization.

If food shape or meaning changes, update both mapper directions, potentially add a tests for it.\
Editing only the SQL entity or only the domain model silently loses data.
