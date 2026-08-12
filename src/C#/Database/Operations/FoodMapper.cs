using Food_Database.Database.Descriptors;
using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Food_Database.Database.Operations
{

    using Food;
    using Food_Database.Database.Descriptors;

    public static class FoodMapper
    {
        private const double Unknown = -1d;

        public static Food ToDomain(
            this Food_DBEntity entity,
            double weight = DB_Food_Descriptors.NormalizedWeight)
        {
            double factor = weight / DB_Food_Descriptors.NormalizedWeight;

            Nutrients nutrients = CreateNutrients(entity);

            // if it is 1 nothing changes
            nutrients = factor * nutrients;

            return new Food(
                id: entity.Id,
                name: entity.Name,
                weight: weight,
                nutrientContent: nutrients,
                description: entity.Food_Description ?? string.Empty
            );
        }

        public static Food ToDomain(
            this Food_DBEntity entity,
            UserFoodOptions_DBEntity? userOptions)
        {
            double weight = userOptions?.Weight_Total is decimal userWeight
                ? (double)userWeight
                : DB_Food_Descriptors.NormalizedWeight;

            return entity.ToDomain(weight);
        }

        public static Food ToDomainWithIngredients(
            this Food_DBEntity entity,
            double weight = DB_Food_Descriptors.NormalizedWeight)
        {
            return ToDomainWithIngredientsInternal(
                entity,
                weight,
                new HashSet<int>());
        }

        private static Food ToDomainWithIngredientsInternal(
            Food_DBEntity entity,
            double weight,
            HashSet<int> path)
        {
            var food = entity.ToDomain(weight);

            // Prevent recursive A -> B -> A composition from blowing the stack.
            if (!path.Add(entity.Id))
                return food;

            double parentFactor = weight / DB_Food_Descriptors.NormalizedWeight;

            foreach (FoodIngredients_DBEntity relation in entity.FoodIngredientsFood)
            {
                double ingredientWeight =
                    (double)relation.Weight_Ingredient_Normalised
                    * parentFactor;

                Food ingredient = ToDomainWithIngredientsInternal(
                    relation.Ingredient_Food,
                    ingredientWeight,
                    path);

                food.AddIngredient(ingredient);
            }

            path.Remove(entity.Id);

            return food;
        }

        /// <summary>
        /// Creates <see cref="Nutrients"> object from <see cref="Food_DBEntity"/> and rounds it up to 2 places as database logic.
        /// </summary>
        /// <remark>Because of nature where this is used -- creating from db object that shouldnt be manipulated --
        /// this is probably not necessary because it should be 2 decimal places trunc by database.</remark>
        /// <param name="entity">Database entity in EntityFramework</param>
        /// <returns><see cref="Nutrients"/> object with rounded up decimals.</returns>
        private static Nutrients CreateNutrients(Food_DBEntity entity)
        {
            Nutrients nut = new Nutrients(
                new Energy(Value(entity.Energy_Kcal)),
                new Fat(
                    Value(entity.Fat_Total),
                    Value(entity.Fat_Saturated)),
                new Carbohydrates(
                    Value(entity.Carbs_Total),
                    Value(entity.Carbs_Sugar)),
                new Protein(
                    Value(entity.Protein_Total)),
                new Salt(
                    Value(entity.Salt_Total))
            );

            return nut.RoundUp2decimal();
        }

        public static void MapToEntity(
            Food food,
            Food_DBEntity entity)
        {
            if (food.Weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(food.Weight));

            Nutrients nutrientsPer100g =
                (DB_Food_Descriptors.NormalizedWeight / food.Weight) * food.NutrientContent;

            entity.Name = food.Name;
            entity.Food_Description = string.IsNullOrWhiteSpace(food.Description)
                ? null
                : food.Description;

            entity.Energy_Kcal = (int?)Value(nutrientsPer100g.Energy.Kcal);

            entity.Fat_Total = Value(nutrientsPer100g.FatContent.Total);
            entity.Fat_Saturated = Value(nutrientsPer100g.FatContent.Saturated);

            entity.Carbs_Total = Value(nutrientsPer100g.CarbohydrateContent.Total);
            entity.Carbs_Sugar = Value(nutrientsPer100g.CarbohydrateContent.Sugar);

            entity.Protein_Total = Value(nutrientsPer100g.Protein.Total);
            entity.Salt_Total = Value(nutrientsPer100g.Salt.Total);
        }

        private static double Value(decimal? value)
            => value.HasValue
                ? (double)value.Value
                : Unknown;

        private static decimal? Value(double value)
        {
            return value < 0
                ? null
                : (decimal)value;
        }
    }
}
