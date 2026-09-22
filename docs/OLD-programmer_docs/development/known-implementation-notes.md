[Programmer documentation](../README.md)

# Known implementation notes

- `SetDescription` has no ownership check and no API endpoint. Yet not needed.
- The current seed file CSV has no salt column, so --dbcheck can fail because nutrient sum can be more than 100g.
- SQL initialization is for a fresh Docker volume. For updating existing table, there is need to create new script.
