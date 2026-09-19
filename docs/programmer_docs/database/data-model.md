[Programmer documentation](../README.md)

# Data model

## Important read
Current `IF OBJECT_ID ... IS NULL` scripts create missing tables but **do not alter tables**.\
If you decide to change database while it has some data in it, and you want to keep them, you need to create upgrade script.\
A schema change needs matching SQL creation-script and EF entity/context changes.\
Rebuilding the API does not change the persistent Docker database volume.

## Important food rule: values are per 100g

`Food` rows always store nutrients per 100g.\
Domain `Food.Weight` may be a different amount, for example to work with 250g package.\

`Mapper` scales values at the database/domain boundary:
- SQL `NULL` means unknown
- domain numeric `-1` means unknown
- saving normalizes to 100g and converts negative/unknown values to `NULL`
- reading converts SQL `NULL` to domain `-1`

When making new functions for working with domain or databse entity, try using previous conventions with `-1` and `NULL`.

## Tables and relationships

| Table | Meaning | Key / relationship |
| --- | --- | --- |
| `Food` | Saves all simple and composite foods; nutrients per 100g. | `Id` primary key. |
| `FoodIngredients` | Mapping: food contains an ingredient food. | `(Food_Id, Ingredient_Food_Id)`; both reference `Food`. |
| `Users` | Application Identity users. | `Id`; `Users_DBEntity : IdentityUser<int>`. |
| `UserCreatedFood` | Mapping: Food can be created by 0 (system food) up to 1 user. | `Food_Id` is PK and FK; many rows may belong to one user. |
| `UserFoodFavorites` | Mapping: User-to-food favorites. | `(User_Id, Food_Id)`; EF navigate `Users_DBEntity.Food`. |
| `UserFoodOptions` | One user's package/serving option, up to N. | `(User_Id, Food_Id, Weight_Total)`, meaning price is optional. |
| `UserFoodRemarks` | Up to one private user remark per food. | `(User_Id, Food_Id)`. |
| `AspNetRoles`, `AspNet*`, etc. | ASP.NET Identity tables, basically copy-pasted from tutorial. | Created in `04_init_identity.sql`. |

`FoodIngredientsFood` is this food's ingredients.\
`FoodIngredientsIngredient_Food` is the composites that use this food.\
SQL prevents a direct self-reference.\
You should write it explicitly in C# application code to prevent errors.\
The `Food.Repository` detects cycles during creation, and mapping/reading guards recursion.

## Ownership and privacy

`Food` is shared data, and `Food_Description` is its general description.\
Remarks, options, and favorites are private user data and must be filtered by the current user.\
`UserCreatedFood` differnetiates system foods from user-created foods. Currently it is used only for filtering.
