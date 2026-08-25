using Food_Database.Database.Descriptors;
using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Food_Database.Database.Operations {
    using Food;
    using static Food.Food;

    /// <summary>
    /// Class for working with entity framework.
    /// </summary>
    public static class EFLoader {
        /// <summary>
        /// Class to work with database Food.
        /// </summary>
        public sealed class FoodRepository {
            /// <summary>
            /// <see cref="DbContext"/> for food database.
            /// </summary>
            private readonly DB_FoodContext _db;

            /// <summary>
            /// Sets <see cref="DbContext"/>.
            /// </summary>
            /// <param name="db">Database connection.</param>
            public FoodRepository(DB_FoodContext db) {
                _db = db;
            }

            /// <summary>
            /// Public call to save database context changes.
            /// </summary>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>Empty <see cref="Task"/>.</returns>
            public async Task SaveChangesDBAsync(CancellationToken cancellationToken = default) {
                await _db.SaveChangesAsync(cancellationToken);
            }

            /// <summary>
            /// Function to get <see cref="Food"/> out of database by <see cref="Food_DBEntity.Id"/>.
            /// </summary>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns><see cref="Food"/> object parsed from <see cref="Food_DBEntity"/>.</returns>
            public async Task<Food?> GetFoodAsync(int foodId, CancellationToken cancellationToken = default) {
                Food_DBEntity? entity = await _db.Food
                    .AsNoTracking()
                    .Include(x => x.FoodIngredientsFood)
                        .ThenInclude(x => x.Ingredient_Food)
                    .SingleOrDefaultAsync(
                        x => x.Id == foodId,
                        cancellationToken);

                return entity?.ToDomainWithIngredients();
            }

            /// <summary>
            /// Function to get <see cref="Food"/> out of database by <see cref="Food_DBEntity.Id"/> for certain <see cref="Users_DBEntity.Id"/>.
            /// </summary>
            /// <param name="foodId">ID of food.</param>
            /// <param name="userId">ID of user.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns><see cref="Food"/> object parsed from <see cref="Food_DBEntity"/>.</returns>
            public async Task<Food?> GetFoodForUserAsync(int foodId, int userId, CancellationToken cancellationToken = default) {
                Food_DBEntity? entity = await _db.Food
                    .AsNoTracking()
                    .Include(x => x.FoodIngredientsFood)
                        .ThenInclude(x => x.Ingredient_Food)
                    .Include(x => x.UserFoodOptions)
                    .SingleOrDefaultAsync(
                        x => x.Id == foodId,
                        cancellationToken);

                if (entity is null) {
                    return null;
                }

                UserFoodOptions_DBEntity? options =
                    entity.UserFoodOptions
                        .SingleOrDefault(x => x.User_Id == userId);

                double weight = options?.Weight_Total is decimal savedWeight
                    ? (double)savedWeight
                    : DB_Food_Descriptors.NormalizedWeight;

                return entity.ToDomainWithIngredients(weight);
            }

            /// <summary>
            /// Get number of <see cref="Food_DBEntity"/> in the database.
            /// </summary>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>Number of <see cref="Food_DBEntity"/> in the database.</returns>
            public async Task<int> GetFoodCountAsync(CancellationToken cancellationToken = default) {
                return
                    await _db.Food.CountAsync(cancellationToken);
            }

            /// <summary>
            /// Function to get <see cref="Food"/> out of database in form of sequence for listing out food in order.
            /// </summary>
            /// <param name="from">Rank of element, non-index form.</param>
            /// <param name="to">Up to what rank of element, non-index form.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns><see cref="Food"/> objects in form of list, in the fixed order by id, from database.</returns>
            public async Task<List<Food>> GetFoodsAsync(int from = 1, int to = 10,
        CancellationToken cancellationToken = default) {
                if (from < 1) {
                    from = 1;
                }

                if (to < from) {
                    return new List<Food>();
                }

                int count = to - from + 1;

                List<Food_DBEntity> entities = await _db.Food
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .Skip(from - 1)
                    .Take(count)
                    .Include(x => x.FoodIngredientsFood)
                        .ThenInclude(x => x.Ingredient_Food)
                    .ToListAsync(cancellationToken);

                return [.. entities.Select(x => x.ToDomainWithIngredients())];
            }

            /// <summary>
            /// Function to get favorite <see cref="Food"/>s out of database for certain <see cref="Users_DBEntity.Id"/>.
            /// </summary>
            /// <param name="userId">ID of user.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>List of <see cref="FavoriteFood"/> objects parsed from <see cref="Food_DBEntity"/>.</returns>
            public async Task<List<FavoriteFood>> GetFavoritesWithOptionsAsync(
                int userId,
                CancellationToken cancellationToken = default) {
                List<Food_DBEntity> user_favorites_entities = await _db.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .SelectMany(x => x.Food)
                    .Include(x => x.UserFoodOptions)
                    .Include(x => x.FoodIngredientsFood)
                        .ThenInclude(x => x.Ingredient_Food)
                    .OrderBy(x => x.Id)
                    .ToListAsync(cancellationToken);

                return
                    [.. user_favorites_entities
                    .Select(food_entity => new FavoriteFood
                    {
                        Food = food_entity.ToDomainWithIngredients(),

                        Options = [.. food_entity.UserFoodOptions
                            .Where(x => x.User_Id == userId)
                            .OrderBy(x => x.Weight_Total)
                            .Select(x => new FavoriteFoodOption
                            {
                                Weight = x.Weight_Total,
                                Price = x.Price_Eur
                            })]
                    })];
            }

            /// <summary>
            /// Function to add <see cref="Food"/> into database to become <see cref="Food_DBEntity"/> and get its ID.
            /// </summary>
            /// <param name="food">Food domain object.</param>
            /// <param name="userId">ID of a user who created food, or set to -1 if not created by user.</param>
            /// <param name="addIngredients">Whether or not also add food ingredients.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>Added <see cref="Food"/> with its ID from database.</returns>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            /// <remarks>Saves databse context.</remarks>
            public async Task<Food> AddFoodAsync(
        Food food,
        int userId = -1,
        bool addIngredients = false,
        CancellationToken cancellationToken = default) {
                Food_DBEntity entity = new();
                FoodMapper.MapToEntityNormalized(new Food(food), entity);
                _db.Food.Add(entity);

                if (userId > 0) {
                    entity.UserCreatedFood = new() {
                        User_Id = userId
                    };
                }

                await SaveChangesDBAsync(cancellationToken);

                food.Id = entity.Id;

                if ((addIngredients) &&
                    !(food.Weight <= 0 || food.Ingredients.Count == 0)) {
                    await SetFoodIngredientsAsync(food, entity,
                                                    cancellationToken);
                    await SaveChangesDBAsync(cancellationToken);
                }

                return food;
            }

            /// <summary>
            /// Helper method to add ingredients of a food into database.
            /// </summary>
            /// <param name="food">Base food that is being added, as domain model.</param>
            /// <param name="foodEntity">Base food that is being added, as database entity.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>Empty <see cref="Task"/>.</returns>
            /// <remarks>Ingredients need to exist already in database.</remarks>
            private async Task SetFoodIngredientsAsync(
    Food food,
    Food_DBEntity foodEntity,
    CancellationToken cancellationToken = default) {
                /*ingredients is something like
                 Id  Weight
                  1   100
                  2   50
                  1   25
                  3   0
                  0   80
                  creates {{ 1, 125 }, { 2, 50 }}*/
                var ingredients = food.Ingredients
                    .Where(x => ((x.Id > 0) && (x.Weight > 0)))
                    .GroupBy(x => x.Id)
                    .Select(x => new {
                        FoodId = x.Key,
                        Weight = x.Sum(y => y.Weight)
                    });

                foreach (var ingredient in ingredients) {
                    // how much would there be if base food is 100g
                    decimal normalizedWeight =
                        (decimal)(ingredient.Weight / food.Weight * DB_Food_Descriptors.NormalizedWeight);

                    var ingredientEntity = new FoodIngredients_DBEntity {
                        Food_Id = foodEntity.Id,
                        Ingredient_Food_Id = ingredient.FoodId,
                        Weight_Ingredient_Normalised = normalizedWeight
                    };

                    foodEntity.FoodIngredientsFood.Add(ingredientEntity);
                }

                await _db.SaveChangesAsync(cancellationToken);
            }

            private async Task<Food> GetOrSetIngredientAsync(
    Food ingredient,
    CancellationToken cancellationToken = default) {
                if (ingredient.Id > 0) {
                    Food_DBEntity? existing = await _db.Food
                        .SingleOrDefaultAsync(
                            x => x.Id == ingredient.Id,
                            cancellationToken);

                    if (existing is null) {
                        throw new InvalidOperationException(
                            $"Ingredient '{ingredient.Name}' has ID {ingredient.Id}, " +
                            $"but that ID does not exist in the database.");
                    } else { 
                        ingredient.Id = existing.Id;
                        return ingredient;
                    }
                }

                return
                    await AddFoodAsync(ingredient,
                                        cancellationToken: cancellationToken);
            }

            /// <summary>
            /// Sets favorite food for a user by combination of userId and foodId.
            /// </summary>
            /// <param name="userId">ID of a user whose food is being managed.</param>
            /// <param name="foodId">ID of food that is being set for a user.</param>
            /// <param name="remark">User remark about food. Not a food description.</param>
            /// <param name="options">Combinations of weight and price of managed food.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>Task of 'Food?' that was created into its database food_entity form.</returns>
            /// <exception cref="ArgumentOutOfRangeException">Occurs when weight or price being set is negative.</exception>
            /// <remarks>Saves database context.</remarks>
            public async Task<Food?> SetFavoriteFoodAsync(
        int userId,
        int foodId,
        string? remark = null,
        IEnumerable<FavoriteFoodOption>? options = null,
        CancellationToken cancellationToken = default) {
                if (!await UserExists(userId, cancellationToken) ||
                        !await FoodExists(foodId, cancellationToken)) {
                    return null;
                }

                Users_DBEntity? userEntity = await _db.Users
                    .Include(x => x.Food)
                    .SingleOrDefaultAsync(
                        x => x.Id == userId,
                        cancellationToken);

                Food_DBEntity? foodEntity = await _db.Food
                    .Include(x => x.FoodIngredientsFood)
                        .ThenInclude(x => x.Ingredient_Food)
                    .SingleOrDefaultAsync(
                        x => x.Id == foodId,
                        cancellationToken);

                if (userEntity is null || foodEntity is null) {
                    return null;
                }

                // Favorite food if it isn't already favorited.
                if (!userEntity.Food.Any(x => x.Id == foodId)) {
                    userEntity.Food.Add(foodEntity);
                }

                if (remark is not null) {
                    await AddOrUpdateRemark(userId, foodId, remark,
                                            cancellationToken);
                }

                if (options is not null) {
                    await AddOrUpdateOptions(userId, foodId, options,
                                            cancellationToken);
                }

                await SaveChangesDBAsync(cancellationToken);

                return foodEntity.ToDomainWithIngredients();
            }

            /// <summary>
            /// Helper function to add or update option of user favorite foods.
            /// </summary>
            /// <param name="userId">ID of a user.</param>
            /// <param name="foodId">Favorite Food ID of a user.</param>
            /// <param name="options">Options to set or update.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>Empty <see cref="Task"/>.</returns>
            /// <exception cref="ArgumentOutOfRangeException">When price or weight is smaller than 0.</exception>
            public async Task AddOrUpdateOptions(int userId, int foodId,
        IEnumerable<FavoriteFoodOption> options,
        CancellationToken cancellationToken = default) {
                foreach (FavoriteFoodOption option in options) {
                    if (option.Weight <= 0 || option.Price < 0) {
                        throw new ArgumentOutOfRangeException(nameof(options));
                    }

                    await EnsureFavoriteExists(userId, foodId, cancellationToken);

                    UserFoodOptions_DBEntity? optionEntity =
                        await _db.UserFoodOptions.SingleOrDefaultAsync(
                            x =>
                                x.User_Id == userId &&
                                x.Food_Id == foodId &&
                                x.Weight_Total == option.Weight,
                                cancellationToken);

                    // if option is null add whole, otherwise just change price
                    if (optionEntity is null) {
                        _db.UserFoodOptions.Add(new UserFoodOptions_DBEntity {
                            User_Id = userId,
                            Food_Id = foodId,
                            Weight_Total = option.Weight,
                            Price_Eur = option.Price
                        });
                    } else {
                        optionEntity.Price_Eur = option.Price;
                    }
                }
            }

            /// <summary>
            /// Helper function to add or update remark of a user's favorite food.
            /// </summary>
            /// <param name="userId">ID of a user.</param>
            /// <param name="foodId">Favorite Food ID of a user.</param>
            /// <param name="remark">Remark to set.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>Empty <see cref="Task"/>.</returns>
            private async Task AddOrUpdateRemark(int userId, int foodId, string remark,
        CancellationToken cancellationToken = default) {
                UserFoodRemarks_DBEntity? remarkEntity =
                    await _db.UserFoodRemark.SingleOrDefaultAsync(
                        x =>
                            x.User_Id == userId &&
                            x.Food_Id == foodId,
                        cancellationToken);

                if (remarkEntity is null) {
                    _db.UserFoodRemark.Add(new UserFoodRemarks_DBEntity {
                        User_Id = userId,
                        Food_Id = foodId,
                        Food_Remark = remark
                    });
                } else {
                    remarkEntity.Food_Remark = remark;
                }
            }

            /// <summary>
            /// Adds user favorite food option for a user by combination of userId and foodId.
            /// <br>If food isn't favorite, it is added as favorite.</br>
            /// <br>If it exists by weight, it adjusts price.</br>
            /// </summary>
            /// <param name="userId">ID of a user whose food is being managed.</param>
            /// <param name="foodId">ID of food that is being set for a user.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>Task of 'Food?' that was created into its database food_entity form.</returns>
            /// <exception cref="ArgumentOutOfRangeException">Occurs when weight or price being set is negative.</exception>
            public async Task AddFavoriteFoodOptionAsync(
    int userId,
    int foodId,
    FavoriteFoodOption option,
    CancellationToken cancellationToken = default) {
                if (!await FavoriteExists(userId, foodId, cancellationToken)) {
                    if (!await EnsureFavoriteExists(userId, foodId, cancellationToken)) {
                        return;
                    }
                }

                await AddOrUpdateOptions(userId, foodId, [option], cancellationToken);
            }

            /// <summary>
            /// Helper function to find out if user and food is already favorite.
            /// </summary>
            /// <param name="userId">ID of a user whose food is being managed.</param>
            /// <param name="foodId">ID of food that is being set for a user.</param>
            /// /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>If user has this food set as favorite.</returns>
            public async Task<bool> FavoriteExists(int userId, int foodId, CancellationToken cancellationToken = default) {
                return
                    await _db.Users
                            .AsNoTracking()
                            .Where(x => x.Id == userId)
                            .SelectMany(x => x.Food)
                            .AnyAsync(x => x.Id == foodId, cancellationToken);
            }

            /// <summary>
            /// Helper function to make sure favorite food for user exists, or can be added for a user.
            /// </summary>
            /// <param name="userId">ID of a user whose food is being managed.</param>
            /// <param name="foodId">ID of food that is being set for a user.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>If favorite food-user was created.</returns>
            public async Task<bool> EnsureFavoriteExists(int userId, int foodId, CancellationToken cancellationToken = default) {
                Users_DBEntity? user = await _db.Users
                    .Include(x => x.Food)
                    .SingleOrDefaultAsync(
                        x => x.Id == userId,
                        cancellationToken);

                Food_DBEntity? food = await _db.Food
                    .SingleOrDefaultAsync(
                        x => x.Id == foodId,
                        cancellationToken);

                if (user is null || food is null) {
                    return false;
                }

                user.Food.Add(food);
                return true;
            }

            /// <summary>
            /// Helper function to see if user exists in the database.
            /// </summary>
            /// <param name="userId">ID of a user.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>True if exists.</returns>
            public async Task<bool> UserExists(int userId, CancellationToken cancellationToken = default) {
                Users_DBEntity? user =
                    await _db.Users
                        .AsNoTracking()
                        .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

                return user is not null;
            }

            /// <summary>
            /// Helper function to see if food exists in the database.
            /// </summary>
            /// <param name="userId">ID of a food.</param>
            /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
            /// <returns>True if exists.</returns>
            public async Task<bool> FoodExists(int foodId, CancellationToken cancellationToken = default) {
                Food_DBEntity? food =
                    await _db.Food
                        .AsNoTracking()
                        .SingleOrDefaultAsync(x => x.Id == foodId, cancellationToken);

                return food is not null;
            }

            public async Task<Food?> CreateFoodFromExistingAsync(
    Dictionary<int, decimal> ingredientWeights,
    string name,
    string description = "",
    CancellationToken cancellationToken = default) {
                if (ingredientWeights.Count == 0) {
                    return null;
                }

                decimal totalWeight = 0;
                Nutrients totalNutrients = new(
                    new Energy(0),
                    new Fat(0, 0),
                    new Carbohydrates(0, 0),
                    new Protein(0),
                    new Salt(0));

                List<Food> ingredients = new();

                foreach (KeyValuePair<int, decimal> ingredientInput in ingredientWeights) {
                    int foodId = ingredientInput.Key;
                    decimal gramsUsed = ingredientInput.Value;

                    if (gramsUsed <= 0) {
                        continue;
                    }

                    Food? food = await GetFoodAsync(foodId,
                                                cancellationToken);

                    if (food is null) {
                        continue;
                    }

                    decimal factor = gramsUsed / (decimal)food.Weight;

                    Food ingredient = new Food(
                        food.Id,
                        food.Name,
                        (double)gramsUsed,
                        factor * food.NutrientContent,
                        food.Description,
                        food.Ingredients);

                    totalWeight += gramsUsed;
                    totalNutrients += ingredient.NutrientContent;

                    ingredients.Add(ingredient);
                }

                if (ingredients.Count == 0) {
                    return null;
                }

                return new Food(
                    id: -1,
                    name: name,
                    weight: NumberOperations.RoundUpTo2DecimalPlaces(totalWeight),
                    nutrientContent: totalNutrients.RoundUp2decimal(),
                    description: description,
                    ingredients: ingredients);
            }

            /// <summary>
            /// Helper function for clamping value<0 to null.
            /// </summary>
            private static decimal? Value(double value) {
                return value < 0
                    ? null
                    : (decimal)value;
            }
        }
    }
}
