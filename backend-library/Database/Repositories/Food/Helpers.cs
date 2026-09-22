using Microsoft.EntityFrameworkCore;

using Food_Database.Models;
using FoodBase;

namespace Food_Database.Database.Repositories.Foods {
    public sealed partial class Repository {
        /// <summary>
        /// Helper function to find out if user and food is already favorite.
        /// </summary>
        /// <param name="userId">ID of a user whose food is being managed.</param>
        /// <param name="foodId">ID of food that is being set for a user.</param>
        /// /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns>If user has this food set as favorite.</returns>
        public async Task<bool> FavoriteExistsAsync(int userId, int foodId, CancellationToken cancellationToken = default) {
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
        public async Task<bool> EnsureFavoriteExistsAsync(int userId, int foodId,
    CancellationToken cancellationToken = default) {
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

            if (!user.Food.Any(x => x.Id == foodId)) {
                user.Food.Add(food);
            }
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
        /// Helper function to get or set single ingredient as <see cref="Food"/>.
        /// </summary>
        /// <param name="ingredient">Food ingredient (without other ingredients).</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns><see cref="Task"/> of <see cref="Food"/> with ID from database.</returns>
        /// <exception cref="InvalidOperationException">When Ingredient ID is same as Base food ID.</exception>
        private async Task<(Food_DBEntity Entity, bool WasAdded)>
        GetOrSetIngredientEntityAsync(
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
                }

                return (existing, false);
            }

            Food added = await AddFoodAsync(
                ingredient,
                addIngredients: false,
                cancellationToken: cancellationToken);

            Food_DBEntity entity = await _db.Food
                .SingleAsync(
                    x => x.Id == added.Id,
                    cancellationToken);

            ingredient.Id = entity.Id;

            return (entity, true);
        }

        /// <summary>
        /// Helper function to get or set ingredient as <see cref="Food"/>.
        /// </summary>
        /// <param name="ingredient">Food ingredient (without other ingredients).</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns><see cref="Task"/> of <see cref="Food"/> with ID from database.</returns>
        /// <exception cref="InvalidOperationException">When Ingredient ID is same as Base food ID.</exception>
        private async Task<Food> GetOrSetIngredientAsync(
Food ingredient,
CancellationToken cancellationToken = default) {
            if (ingredient.Id <= 0) {
                return
                    await AddFoodAsync(ingredient,
                                        cancellationToken: cancellationToken);
            }

            Food_DBEntity? existing = await _db.Food
                .SingleOrDefaultAsync(
                    x => x.Id == ingredient.Id,
                    cancellationToken);

            if (existing is not null) {
                ingredient.Id = existing.Id;
                return ingredient;
            }

            throw new InvalidOperationException(
                $"Ingredient '{ingredient.Name}' has ID {ingredient.Id}, " +
                    $"but that ID does not exist in the database.");
        }

        /// <summary>
        /// Helper function to see if food exists in the database.
        /// </summary>
        /// <param name="userId">ID of a food.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns>True if exists.</returns>
        public async Task<bool> FoodExistsAsync(int foodId,
    CancellationToken cancellationToken = default) {
            Food_DBEntity? food =
                await _db.Food
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == foodId,
                                                cancellationToken);

            return food is not null;
        }

        /// <summary>
        /// Helper function to see if food is created by specific user.
        /// </summary>
        /// <param name="foodId">The ID of the food to check.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns><c>True</c> <see cref="bool"/> <see cref="Task"/> if the food is user-created. Otherwise <c>false</c>.</returns>
        public async Task<bool> IsUserCreatedFoodAsync(
int foodId,
CancellationToken cancellationToken = default) {
            return await _db.UserCreatedFood
                .AsNoTracking()
                .AnyAsync(
                    x => x.Food_Id == foodId,
                    cancellationToken);
        }

        /// <summary>
        /// Helper function to see if food is favorite for specific user.
        /// </summary>
        /// <param name="userId">The ID of user.</param>
        /// <param name="foodId">The ID of food to check.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns><c>True</c> <see cref="bool"/> <see cref="Task"/> if the food is user favorite. Otherwise <c>false</c>.</returns>
        public async Task<bool> IsUserFavoriteFoodAsync(
int userId,
int foodId,
CancellationToken cancellationToken = default) {
            return await _db.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .SelectMany(x => x.Food)
                .AnyAsync(
                    x => x.Id == foodId,
                    cancellationToken);
        }
    }
}
