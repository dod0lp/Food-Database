using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Food_Database.Database.Operations
{
    using Food;



    public static class EFLoader
    {
        public static void NormalizeNegativeValues(Food food)
        {
            if (food.Weight < 0)
                food.Weight = -1;

            object nutrients = food.NutrientContent;
            NormalizeNegativeValuesRecursive(nutrients);
            food.NutrientContent = (Nutrients)nutrients;

            foreach (Food ingredient in food.Ingredients)
                NormalizeNegativeValues(ingredient);
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
                    : 100d;

                return entity.ToDomainWithIngredients(weight);
            }
        }
    }
}
