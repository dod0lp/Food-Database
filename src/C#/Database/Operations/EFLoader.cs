using Food_Database.Database.Descriptors;
using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Food_Database.Database.Operations
{
    using Food;
    using static Food.Food;

    public static class EFLoader
    {
        public static Nutrients NormalizeNutrients(Nutrients nutrients, double weight)
        {
            Nutrients nutrientsPer100g =
                (DB_Food_Descriptors.NormalizedWeight / weight) * nutrients;

            NormalizeNegativeValuesRecursive(nutrientsPer100g);
            return nutrientsPer100g;
        }

        public static void NormalizeFood(Food food)
        {
            if (food.Weight < 0)
                food.Weight = -1;

            object nutrients = food.NutrientContent;
            NormalizeNegativeValuesRecursive(nutrients);
            food.NutrientContent = (Nutrients)nutrients;

            foreach (Food ingredient in food.Ingredients)
                NormalizeFood(ingredient);
        }

        private static void NormalizeNegativeValuesRecursive(object obj)
        {
            Type type = obj.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                object? value = property.GetValue(obj);

                if (value is double number)
                {
                    if (number < 0)
                        property.SetValue(obj, -1d);
                }
                else if (value is not null && property.PropertyType.IsValueType)
                {
                    object nested = value;

                    NormalizeNegativeValuesRecursive(nested);

                    property.SetValue(obj, nested);
                }
            }
        }


        public sealed class FoodRepository
        {
            private readonly DB_FoodContext _db;

            public FoodRepository(DB_FoodContext db)
            {
                _db = db;
            }

            /// <summary>
            /// Function to get <see cref="Food"/> out of database by <see cref="Food_DBEntity.Id"/>.
            /// </summary>
            /// <param name="cancellationToken">CancellationToken for async op.</param>
            /// <returns><see cref="Food"/> object parsed from <see cref="Food_DBEntity"/>.</returns>
            public async Task<Food?> GetFoodAsync(int foodId, CancellationToken cancellationToken = default)
            {
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
            public async Task<Food?> GetFoodForUserAsync(int foodId, int userId, CancellationToken cancellationToken = default)
            {
                Food_DBEntity? entity = await _db.Food
                    .AsNoTracking()
                    .Include(x => x.FoodIngredientsFood)
                        .ThenInclude(x => x.Ingredient_Food)
                    .Include(x => x.UserFoodOptions)
                    .SingleOrDefaultAsync(
                        x => x.Id == foodId,
                        cancellationToken);

                if (entity is null)
                    return null;

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
            public async Task<int> GetFoodCountAsync(CancellationToken cancellationToken = default)
            {
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
                CancellationToken cancellationToken = default)
            {
                if (from < 1)
                {
                    from = 1;
                }

                if (to < from)
                {
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
                CancellationToken cancellationToken = default)
            {
                List<Food_DBEntity> entities = await _db.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .SelectMany(x => x.Food)
                    .Include(x => x.UserFoodOptions)
                    .Include(x => x.FoodIngredientsFood)
                        .ThenInclude(x => x.Ingredient_Food)
                    .OrderBy(x => x.Id)
                    .ToListAsync(cancellationToken);

                return
                    [.. entities
                    .Select(entity => new FavoriteFood
                    {
                        Food = entity.ToDomainWithIngredients(),

                        Options = [.. entity.UserFoodOptions
                            .Where(x => x.User_Id == userId)
                            .OrderBy(x => x.Weight_Total)
                            .Select(x => new FavoriteFoodOption
                            {
                                Weight = (double)x.Weight_Total,
                                PriceEur = x.Price_Eur
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
                CancellationToken cancellationToken = default)
            {
                NormalizeFood(food);
                Nutrients nutrientsPer100g = food.NutrientContent;

                var entity = new Food_DBEntity
                {
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

                await _db.SaveChangesAsync(cancellationToken);

                food.Id = entity.Id;

                return food;
            }

            /// <summary>
            /// Helper function for clamping value<0 to null.
            /// </summary>
            private static decimal? Value(double value)
            {
                return value < 0
                    ? null
                    : (decimal)value;
            }
        }
    }
}
