using Food_Database.Database.Descriptors;
using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Food_Database.Database.Operations {
    using Food;
    using Microsoft.Extensions.Options;
    using System.Runtime.CompilerServices;
    using static Food.Food;

    /// <summary>
    /// Class for working with entity framework.
    /// </summary>
    public static class EFLoader {
        /// <summary>
        /// Normalize nutrinets to 100g weight of food and set to -1 if there is some negative value, as not set in this applicaton logic.
        /// </summary>
        /// <param name="nutrients">Nutrients to normalize.</param>
        /// <param name="weight">Weight of current food</param>
        /// <returns>Normalized Nutrients to 100g worth of food, with -1 values as non-set.</returns>
        public static Nutrients NormalizeNutrients(Nutrients nutrients, double weight) {
            Nutrients nutrientsPer100g =
                (DB_Food_Descriptors.NormalizedWeight / weight) * nutrients;

            NormalizeNegativeValuesRecursive(nutrientsPer100g);
            return nutrientsPer100g;
        }

        /// <summary>
        /// Function to normalize food nutrients, and normalize food it is made out of
        /// </summary>
        /// <remarks>Normalize food to 100g weight as stored in database.</remarks>
        /// <param name="food">Food object to normalize its nutrients.</param>
        public static void NormalizeFood(Food food) {
            // Probably not exception because food obj should be checked earlier by app logic.
            if (food.Weight <= 0) {
                return;
            }

            Nutrients nutrients = new(food.NutrientContent);
            food.NutrientContent = NormalizeNutrients(nutrients, food.Weight);

            foreach (Food ingredient in food.Ingredients) {
                NormalizeFood(ingredient);
            }
        }

        /// <summary>
        /// Recursive function to set each negative property of an object to be -1.
        /// </summary>
        /// <param name="obj">Object to set negative numeric properties to -1.</param>
        private static void NormalizeNegativeValuesRecursive(object obj) {
            Type type = obj.GetType();

            foreach (PropertyInfo property in type.GetProperties()) {
                if (!property.CanRead || !property.CanWrite) {
                    continue;
                }

                object? value = property.GetValue(obj);

                if (value is double number) {
                    if (number < 0) {
                        property.SetValue(obj, -1d);
                    }
                } else if (value is not null && property.PropertyType.IsValueType) {
                    object nested = value;

                    NormalizeNegativeValuesRecursive(nested);

                    property.SetValue(obj, nested);
                }
            }
        }


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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
            /// <returns>Empty <see cref="Task"/>.</returns>
            public async Task SaveChangesDBAsync(CancellationToken cancellationToken = default) {
                await _db.SaveChangesAsync(cancellationToken);
            }

            /// <summary>
            /// Function to get <see cref="Food"/> out of database by <see cref="Food_DBEntity.Id"/>.
            /// </summary>
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
            /// <returns>Added <see cref="Food"/> with its ID from database.</returns>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public async Task<Food> AddFoodAsync(
            Food food,
            CancellationToken cancellationToken = default) {
                NormalizeFood(food);
                Nutrients nutrientsPer100g = food.NutrientContent;

                var entity = new Food_DBEntity {
                    Name = food.Name,

                    Food_Description = string.IsNullOrWhiteSpace(food.Description)
                        ? null
                        : food.Description,

                    Energy_Kcal = (int?)Value(nutrientsPer100g.Energy.Kcal),

                    Fat_Total = Value(nutrientsPer100g.FatContent.Total),
                    Fat_Saturated = Value(nutrientsPer100g.FatContent.Saturated),

                    Carbs_Total = Value(nutrientsPer100g.CarbohydrateContent.Total),
                    Carbs_Sugar = Value(nutrientsPer100g.CarbohydrateContent.Sugar),

                    Protein_Total = Value(nutrientsPer100g.Protein.Total),
                    Salt_Total = Value(nutrientsPer100g.Salt.Total)
                };

                _db.Food.Add(entity);

                await SaveChangesDBAsync(cancellationToken);

                food.Id = entity.Id;

                return food;
            }

            /// <summary>
            /// Sets favorite food for a user by combination of userId and foodId.
            /// </summary>
            /// <param name="userId">ID of a user whose food is being managed.</param>
            /// <param name="foodId">ID of food that is being set for a user.</param>
            /// <param name="remark">User remark about food. Not a food description.</param>
            /// <param name="options">Combinations of weight and price of managed food.</param>
            /// <param name="cancellationToken">CancellationToken for async op.</param>
            /// <returns>Task of 'Food?' that was created into its database food_entity form.</returns>
            /// <exception cref="ArgumentOutOfRangeException">Occurs when weight or price being set is negative.</exception>
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

                // Add/update user's remark.
                if (remark is not null) {
                    await AddOrUpdateRemark(userId, foodId, remark,
                                            cancellationToken);
                }

                // Add/update weight + price options.
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
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
            /// <param name="cancellationToken">CancellationToken for async op.</param>
            /// <returns>True if exists.</returns>
            public async Task<bool> FoodExists(int foodId, CancellationToken cancellationToken = default) {
                Food_DBEntity? food =
                    await _db.Food
                        .AsNoTracking()
                        .SingleOrDefaultAsync(x => x.Id == foodId, cancellationToken);

                return food is not null;
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
