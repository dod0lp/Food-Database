using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using FoodBase;
using static FoodBase.Food;
using Food_Database.Database.Descriptors;
using Food_Database.Models;

namespace Food_Database.Database.Repositories.Foods {
    public sealed partial class Repository {
        /// <summary>
        /// Function to add <see cref="Food"/> into database to become <see cref="Food_DBEntity"/> and get its ID.
        /// </summary>
        /// <param name="food">Food domain object.</param>
        /// <param name="userId">ID of a user who created food, or set to -1 if not created by user.</param>
        /// <param name="addIngredients">Whether or not also add food ingredients.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns>Added <see cref="Food"/> with its ID from database.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <remarks>By default without ingredients. Saves databse context.</remarks>
        public async Task<Food> AddFoodAsync(
    Food food,
    int userId = -1,
    bool addIngredients = false,
    CancellationToken cancellationToken = default) {
            Food_DBEntity entity = new();
            Mapper.MapToEntityNormalized(new Food(food), entity);
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
        /// <remarks>Ingredients do <b>not</b> need to exist already in database.</remarks>
        /// <exception cref="InvalidOperationException">When application logic is broken.</exception>
        private async Task SetFoodIngredientsAsync(
Food food,
Food_DBEntity foodEntity,
CancellationToken cancellationToken = default) {
            await SetFoodIngredientsRecursiveAsync(
                food,
                foodEntity,
                new HashSet<int>(),
                cancellationToken);
        }

        /// <summary>
        /// Helper function to add food ingredients recursively into database, while checking for circular relations.
        /// </summary>
        /// <param name="food">Base food that is being added, as domain model.</param>
        /// <param name="foodEntity">Base food that is being added, as database entity.</param>
        /// <param name="path">HashSet of food IDs to track recursion and circular relations.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns>Empty <see cref="Task"/>.</returns>
        /// <remarks>Ingredients do <b>not</b> need to exist already in database.</remarks>
        /// <exception cref="InvalidOperationException">When application logic is broken.</exception>
        private async Task SetFoodIngredientsRecursiveAsync(
Food food,
Food_DBEntity foodEntity,
HashSet<int> path,
CancellationToken cancellationToken) {
            if (food.Weight <= 0 || food.Ingredients.Count == 0) {
                return;
            }

            if (!path.Add(foodEntity.Id)) {
                throw new InvalidOperationException(
                    $"Circular food ingredient relation detected at food ID {foodEntity.Id}.");
            }

            // add relation, if they don't exist in database
            foreach (Food ingredient in food.Ingredients) {
                if (ingredient.Weight <= 0) {
                    continue;
                }

                var (ingredientEntity, ingredientWasAdded) =
                    await
                        GetOrSetIngredientEntityAsync(ingredient,
                                                    cancellationToken);

                if (ingredientEntity.Id == foodEntity.Id) {
                    throw new InvalidOperationException(
                        "Food cannot contain itself as an ingredient.");
                }

                if (path.Contains(ingredientEntity.Id)) {
                    throw new InvalidOperationException(
                        $"Circular food ingredient relation detected: " +
                        $"{foodEntity.Id} -> {ingredientEntity.Id}.");
                }

                decimal normalizedWeight = (decimal)
                    ((ingredient.Weight / food.Weight) *
                    DB_Food_Descriptors.NormalizedWeight);

                FoodIngredients_DBEntity? existingRelation =
                    await _db.FoodIngredients
                        .SingleOrDefaultAsync(
                            x =>
                                x.Food_Id == foodEntity.Id &&
                                x.Ingredient_Food_Id == ingredientEntity.Id,
                            cancellationToken);

                if (existingRelation is null) {
                    foodEntity.FoodIngredientsFood.Add(
                        new FoodIngredients_DBEntity {
                            Food_Id = foodEntity.Id,
                            Ingredient_Food_Id = ingredientEntity.Id,
                            Weight_Ingredient_Normalised = normalizedWeight
                        });
                } else {
                    existingRelation.Weight_Ingredient_Normalised =
                        normalizedWeight;
                }

                // Recurse only when this ingredient was newly inserted.
                // If it already existed in DB, assumed to already be stored.
                if (ingredientWasAdded &&
                        ingredient.Ingredients.Count > 0) {
                    await SetFoodIngredientsRecursiveAsync(
                        ingredient,
                        ingredientEntity,
                        path,
                        cancellationToken);
                }
            }

            path.Remove(foodEntity.Id);
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

            if (remark is not null && remark.Length > 0) {
                await AddOrUpdateRemark(userId, foodId, remark,
                                        cancellationToken);
            }

            if (options is not null && !options.IsNullOrEmpty()) { // VS warning says it can be null when i use only !nullorempty()... huh
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
        /// Creates composite food from existing foods.
        /// </summary>
        /// <param name="ingredientWeights">Dictionary mapping food IDs to weights.</param>
        /// <param name="name">Name of the composite food.</param>
        /// <param name="description">Description of the composite food.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns>The created composite food or null if creation failed.</returns>
        /// <remarks>Description should be only for system made food, otherwise use user-mapped remark.</remarks>
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

            // create ingredients and calculate total weight and nutrients
            foreach (var ingredientInput in ingredientWeights) {
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

                Food ingredient = new(
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
        /// Sets description of a food in the database.
        /// </summary>
        /// <param name="foodId">ID of a food to update description for.</param>
        /// <param name="description">The new description for the food. <b>null</b> to remove the description.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns><b>true</b> if the description was updated successfully, <c>false</c> otherwise.</returns>
        public async Task<bool> SetDescription(int foodId, string? description = null, CancellationToken cancellationToken = default) {
            int rowsAffected = await _db.Food
                .Where(x => x.Id == foodId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(f => f.Food_Description, description),
                    cancellationToken);

            return rowsAffected > 0;
        }
    }
}
