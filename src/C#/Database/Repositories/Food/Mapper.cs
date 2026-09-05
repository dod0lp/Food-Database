namespace Food_Database.Database.Repositories.Foods {

    using FoodBase;
    using Food_Database.Database.Descriptors;
    using Food_Database.Models;
    using System.Reflection;

    /// <summary>
    /// Static class that provides mapping functions between database entities and domain models for food-related data.
    /// </summary>
    public static class Mapper {
        /// <summary>
        /// Constant to represent unknown value for nutrients.
        /// </summary>
        private const double Unknown = -1d;

        /// <summary>
        /// Maps a <see cref="Food_DBEntity"/> to a <see cref="Food"/> domain model, adjusting nutrient values based on the specified weight.
        /// </summary>
        /// <param name="entity">The database entity to map.</param>
        /// <param name="weight">The weight to adjust the nutrient values.</param>
        /// <returns>The mapped <see cref="Food"/> domain model.</returns>
        /// <remarks>Doesn' map ingredients.</remarks>
        public static Food ToDomain(
    this Food_DBEntity entity,
    double weight = DB_Food_Descriptors.NormalizedWeight) {
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

        /// <summary>
        /// Maps a <see cref="Food_DBEntity"/> to a <see cref="Food"/> domain model,
        /// adjusting nutrient values based on the weight specified in <see cref="UserFoodOptions_DBEntity"/>.
        /// </summary>
        /// <param name="entity">The database entity to map.</param>
        /// <param name="userOptions">The user options containing the specified weight.</param>
        /// <returns>The mapped <see cref="Food"/> domain model.</returns>
        /// <remarks>Doesn't map ingredients.</remarks>
        public static Food ToDomain(
    this Food_DBEntity entity,
    UserFoodOptions_DBEntity? userOptions) {
            double weight = userOptions?.Weight_Total is decimal userWeight
                ? (double)userWeight
                : DB_Food_Descriptors.NormalizedWeight;

            return entity.ToDomain(weight);
        }

        /// <summary>
        /// Maps a <see cref="Food_DBEntity"/> to a <see cref="Food"/> domain model, including its ingredients.
        /// </summary>
        /// <param name="entity">The database entity to map.</param>
        /// <param name="weight">The weight to adjust the nutrient values.</param>
        /// <returns>The mapped <see cref="Food"/> domain model.</returns>
        public static Food ToDomainWithIngredients(
    this Food_DBEntity entity,
    double weight = DB_Food_Descriptors.NormalizedWeight) {
            return ToDomainWithIngredientsInternal(
                entity,
                weight,
                new HashSet<int>());
        }

        /// <summary>
        /// Internal recursive function to map a <see cref="Food_DBEntity"/> to a <see cref="Food"/> domain model,
        /// </summary>
        /// <param name="entity">The database entity to map.</param>
        /// <param name="weight">The weight to adjust the nutrient values.</param>
        /// <param name="path">The set of visited entity IDs (to prevent recursion).</param>
        /// <returns>The mapped <see cref="Food"/> domain model.</returns>
        private static Food ToDomainWithIngredientsInternal(
    Food_DBEntity entity,
    double weight,
    HashSet<int> path) {
            var food = entity.ToDomain(weight);

            // Prevent recursive A -> B -> A composition from blowing the stack.
            if (!path.Add(entity.Id)) {
                return food;
            }

            double parentFactor = weight / DB_Food_Descriptors.NormalizedWeight;

            foreach (FoodIngredients_DBEntity relation in entity.FoodIngredientsFood) {
                double ingredientWeight =
                    (double)relation.Weight_Ingredient_Normalised
                    * parentFactor;

                Food ingredient = ToDomainWithIngredientsInternal(
                    relation.Ingredient_Food,
                    ingredientWeight,
                    path
                );

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
        private static Nutrients CreateNutrients(Food_DBEntity entity) {
            Nutrients nutrients = new Nutrients(
                new Energy(ValueOrUnknown(entity.Energy_Kcal)),
                new Fat(
                    ValueOrUnknown(entity.Fat_Total),
                    ValueOrUnknown(entity.Fat_Saturated)),
                new Carbohydrates(
                    ValueOrUnknown(entity.Carbs_Total),
                    ValueOrUnknown(entity.Carbs_Sugar)),
                new Protein(
                    ValueOrUnknown(entity.Protein_Total)),
                new Salt(
                    ValueOrUnknown(entity.Salt_Total))
            );

            return nutrients.RoundUp2decimal();
        }

        /// <summary>
        /// Function that maps <see cref="Food"/> properties to <see cref="Food_DBEntity"/>.
        /// </summary>
        /// <param name="food">Food which parameters to map to entity.</param>
        /// <param name="entity">Entity to map parameters to.</param>
        /// <remarks>Doesn't map ingredients nor ID.</remarks>
        public static void MapToEntityNormalized(Food food, Food_DBEntity entity) {
            Nutrients nutrientsPer100g = NormalizeFood(new Food(food)).NutrientContent;

            entity.Name = food.Name;
            entity.Food_Description = string.IsNullOrWhiteSpace(food.Description)
                ? null
                : food.Description;

            entity.Energy_Kcal = (int?)ValueOrNull(nutrientsPer100g.Energy.Kcal);

            entity.Fat_Total = ValueOrNull(nutrientsPer100g.FatContent.Total);
            entity.Fat_Saturated = ValueOrNull(nutrientsPer100g.FatContent.Saturated);

            entity.Carbs_Total = ValueOrNull(nutrientsPer100g.CarbohydrateContent.Total);
            entity.Carbs_Sugar = ValueOrNull(nutrientsPer100g.CarbohydrateContent.Sugar);

            entity.Protein_Total = ValueOrNull(nutrientsPer100g.Protein.Total);
            entity.Salt_Total = ValueOrNull(nutrientsPer100g.Salt.Total);
        }

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
        /// Function to normalize food nutrients, and normalize food it is made out of.
        /// </summary>
        /// <returns>Food object with normalized values from provided food.</returns>
        /// <remarks>Normalize food to 100g weight as stored in database.</remarks>
        /// <param name="foodBase">Food object to normalize its nutrients.</param>
        public static Food NormalizeFood(Food foodBase) {
            Food food = new(foodBase);
            Nutrients nutrients = new(food.NutrientContent);
            food.NutrientContent = NormalizeNutrients(nutrients, food.Weight);

            foreach (Food ingredient in food.Ingredients) {
                NormalizeFood(ingredient);
            }

            return food;
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
        /// Helper function to convert nullable decimal to double, returning -1 if null.
        /// </summary>
        private static double ValueOrUnknown(decimal? value)
            => value.HasValue
                ? (double)value.Value
                : Unknown;

        /// <summary>
        /// Helper function to convert double to nullable decimal, clamping to null if negative.
        /// </summary>
        private static decimal? ValueOrNull(double value) {
            return value < 0
                ? null
                : (decimal)value;
        }
    }
}
