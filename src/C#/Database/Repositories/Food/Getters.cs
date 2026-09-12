using Microsoft.EntityFrameworkCore;

using FoodBase;
using static FoodBase.Food;
using Food_Database.Database.Descriptors;
using Food_Database.Models;

namespace Food_Database.Database.Repositories.Foods {
    public sealed partial class Repository {
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

        /// <summary>Gets one page of foods supplied by the system, not users.</summary>
        public Task<List<Food>> GetSystemFoodsAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default) =>
            GetFoodPageAsync(
                _db.Food.Where(x => x.UserCreatedFood == null),
                page,
                pageSize,
                cancellationToken);

        /// <summary>Gets one page of foods created by one user.</summary>
        public Task<List<Food>> GetUserCreatedFoodsAsync(
    int userId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default) =>
            GetFoodPageAsync(
                _db.Food.Where(x =>
                    x.UserCreatedFood != null && x.UserCreatedFood.User_Id == userId),
                page,
                pageSize,
                cancellationToken);

        public Task<int> GetSystemFoodCountAsync(CancellationToken cancellationToken = default) =>
            _db.Food.CountAsync(x => x.UserCreatedFood == null, cancellationToken);

        public Task<int> GetUserCreatedFoodCountAsync(
    int userId,
    CancellationToken cancellationToken = default) =>
            _db.Food.CountAsync(x =>
                x.UserCreatedFood != null && x.UserCreatedFood.User_Id == userId,
                cancellationToken);

        private async Task<List<Food>> GetFoodPageAsync(
    IQueryable<Food_DBEntity> query,
    int page,
    int pageSize,
    CancellationToken cancellationToken) {
            List<Food_DBEntity> entities = await query
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                .AsSplitQuery()
                .Include(x => x.UserFoodOptions)
                .Include(x => x.UserFoodRemark)
                .Include(x => x.FoodIngredientsFood)
                    .ThenInclude(x => x.Ingredient_Food)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

            return
                [.. user_favorites_entities
                    .Select(food_entity => new FavoriteFood
                    {
                        Food = food_entity.ToDomainWithIngredients(),

                        Remark = food_entity.UserFoodRemark
                            .SingleOrDefault(x => x.User_Id == userId)
                            ?.Food_Remark,

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
        /// Gets ingredients of a food from the database.
        /// </summary>
        /// <param name="foodId">The ID of the food for which to get ingredients.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns>A <see cref="Task"/> list of <see cref="FoodIngredients_DBEntity"/>.</returns>
        /// <remarks>For ingredients of ingredients call recursively (or iteratively w.e.).</remarks>
        public async Task<List<FoodIngredients_DBEntity>> GetIngredientsReadOnly(int foodId,
    CancellationToken cancellationToken = default) {
            List<FoodIngredients_DBEntity> ingredients =
            await _db.FoodIngredients
                .AsNoTracking()
                .Include(x => x.Ingredient_Food)
                .Where(x => x.Food_Id == foodId)
                .OrderBy(x => x.Ingredient_Food_Id)
                .ToListAsync(cancellationToken);

            return ingredients;
        }
    }
}
