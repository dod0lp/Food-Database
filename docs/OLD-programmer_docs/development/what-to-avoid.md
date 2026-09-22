[Programmer documentation](../README.md)

# What to avoid

- querying `DB_FoodContext` directly from controllers
- returning EF entities as public contracts
- relying only on Angular validation
- treating a user remark as `Food_Description`
- storing non-normalized nutrient values in `Food`
