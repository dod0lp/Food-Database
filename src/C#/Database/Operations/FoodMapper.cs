using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Food_Database.Database.Operations
{
    using Food;

    public static class FoodMapper
    {
        private const double Unknown = -1d;
        private const double NormalizedWeight = 100d;

        public static Food ToDomain(
            this Food_DBEntity entity,
            double weight = NormalizedWeight)
        {
            double factor = weight / NormalizedWeight;

            Nutrients nutrients = CreateNutrients(entity);

            if (factor != 1d)
            {
                nutrients = factor * nutrients;
            }

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
                : NormalizedWeight;

            return entity.ToDomain(weight);
        }

        public static Food ToDomainWithIngredients(
            this Food_DBEntity entity,
            double weight = NormalizedWeight)
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

            double parentFactor = weight / NormalizedWeight;

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

        private static Nutrients CreateNutrients(Food_DBEntity entity)
        {
            return new Nutrients(
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
        }

        private static double Value(int? value)
            => value.HasValue
                ? value.Value
                : Unknown;

        private static double Value(decimal? value)
            => value.HasValue
                ? (double)value.Value
                : Unknown;
    }
}
