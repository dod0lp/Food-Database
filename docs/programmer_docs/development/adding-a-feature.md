[Programmer documentation](../README.md)

# Adding a feature

For stored-food changes, use this order.

0. Use new git branch, also need clean to database volume, or set new one.
1. Define everything: shared/private data, units, unknown/default behaviour, ownership, and deletion behaviour.
2. Change files, probably add a new one, `infrastructure/database/init-scripts/`. Ideally plan an existing-db upgrade.
3. Match it in EF entity classes and `DB_FoodContext.OnModelCreating`.
4. Update domain types and `Mapper`, keeping 100g and `NULL`/`-1` rules.
5. Add/extend a repository operation with validation, async EF calls, and cancellation token.
6. Test database logic, can be done simply through `Main.cs`. Even better -- add automated tests.
7. Add API contract/controller action.
8. Update Angular services, forms/components, errors.
9. Test the Docker end-to-end flow.
10. Test user privacy, their own options set, etc.
11. Update this documentation and `docs/user stories.md` when behaviour changes.
