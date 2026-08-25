namespace Food {
    // There is no check if some values are negative, for example negative amount of Protein, Fat...
    // If there is value not set, the default value in most, if not all, cases will be 0
    // for easier setting up of values, so we can't say when value was not set, but we can set
    // value of something to -1 to explicitly specify, that this value is not set yet

    /// <summary>
    /// Class describing Food - each food has its <see cref="Ingredients"/> - it is simply other <see cref="Food"/>
    /// </summary>
    /// <remarks>
    /// - Contains vague description of database. Database model is defined in <see cref="oldFood_Database.Base"/>
    /// </remarks>
    public class Food {
        /// <summary>
        /// Gets or sets the unique identifier for the <see cref="Food"/> item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the <see cref="Food"/> item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Weight"/> of the <see cref="Food"/> item in grams.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Gets or sets the nutrient content of the <see cref="Food"/>, which contains information such as energy, fat, carbohydrates, and more.
        /// </summary>
        public Nutrients NutrientContent { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Ingredients"/> for the <see cref="Food"/> item. Each ingredient is another <see cref="Food"/> object.
        /// </summary>
        public List<Food> Ingredients { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Description"/> of the <see cref="Food"/> item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Food"/> class.
        /// </summary>
        /// <remarks>
        /// - Should not be used as an instance. Only exists because of compatibility issues.
        /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Food() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        /// Initializes a new instance of the <see cref="Food"/> class with specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the food.</param>
        /// <param name="name">The name of the food.</param>
        /// <param name="weight">The weight of the food in grams.</param>
        /// <param name="nutrientContent">The nutrient content of the food.</param>
        /// <param name="description">A description of the food.</param>
        public Food(int id, string name, double weight, Nutrients nutrientContent, string description) {
            // Main table - Food
            Id = id;
            Name = name;
            Description = description ?? "";
            Weight = NumberOperations.RoundUpTo2DecimalPlaces(weight);

            // Separate table, foreign key ID - Nutrients
            NutrientContent = nutrientContent;
            // NutrientContent = nutrientContent ?? new Nutrients(new(0), new(0), new(0), new(0), new(0));

            // Separate table, foreign key combination of food and other food ID, where first one will be main food,
            // other ID will be one ingredient - other food - food is simply an ingredient
            // - Ingredients
            Ingredients = new List<Food>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Food"/> class with specified parameters and ingredients.
        /// </summary>
        /// <param name="id">The unique identifier for the food.</param>
        /// <param name="name">The name of the food.</param>
        /// <param name="weight">The weight of the food in grams.</param>
        /// <param name="nutrientContent">The nutrient content of the food.</param>
        /// <param name="description">A description of the food.</param>
        /// <param name="ingredients">A list of other food items that are ingredients of this food.</param>
        public Food(int id, string name, double weight, Nutrients nutrientContent, string description, List<Food> ingredients)
            : this(id, name, weight, nutrientContent, description) {
            Ingredients = ingredients;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Food"/> with other <see cref="Food"/>.
        /// </summary>
        /// <remarks>Essentially deep copy.</remarks>
        public Food(Food other) {
            Id = other.Id;
            Name = other.Name;
            Description = other.Description;
            Weight = other.Weight;

            NutrientContent = new Nutrients(other.NutrientContent);

            Ingredients = [..
                other.Ingredients
                    .Select(ingredient => new Food(ingredient))
            ];
        }

        /// <summary>
        /// Adds an ingredient to the list of <see cref="Ingredients"/> for this <see cref="Food"/>.
        /// </summary>
        /// <remarks>
        /// - The ingredient is simply other <see cref="Food"/>.
        /// </remarks>
        /// <param name="food">The food item to add as an ingredient.</param>
        public void AddIngredient(Food food) {
            Ingredients.Add(food);
        }

        /// <summary>
        /// Removes an ingredient from the list of ingredients by its ID.
        /// </summary>
        /// <param name="foodId">The ID of the food item to remove from the ingredients list.</param>
        /// <remarks>
        /// - Barely used for database work.
        /// </remarks>
        /// <returns>True if the ingredient was found and removed; otherwise, false.</returns>
        public bool RemoveIngredient(int foodId) {
            var ingredient = Ingredients.FirstOrDefault(f => f.Id == foodId);

            if (ingredient != null) {
                Ingredients.Remove(ingredient);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts the food object into a human-readable string format.
        /// </summary>
        /// <param name="food">The food object to convert to a readable string.</param>
        /// <returns>A string containing detailed information about the food and its ingredients.</returns>
        public static string ToReadableString(Food food) {
            string foodInfo = $"ID: {food.Id}\nName: {food.Name}\nWeight: {food.Weight}\nNutrients: {food.NutrientContent}"
                +
                $"Description: {food.Description}\n";


            string foodsContained = "Contains: ";
            if (food.Ingredients != null) {
                foodsContained = string.Join("\n", food.Ingredients.Select(food => food.Name));
            }

            if (foodsContained == "Contains: ") {
                foodsContained = "";
            }

            return foodInfo + foodsContained;
        }

        /// <summary>
        /// Converts a food object into a list of strings representing its properties.
        /// </summary>
        /// <param name="food">The food object to convert.</param>
        /// <returns>A list of strings representing the <see cref="Food"/>'s properties and ingredients in human-readable format but in <see cref="List{Food}"/></returns>
        public static List<string> ToStringList(Food food) {
            // Create an array of strings with the specific format
            List<string> foodInfoList = new()
            {
                $"{food.Id}",
                $"{food.Name}",
                $"{food.Weight}",
                $"{food.NutrientContent.Energy.Kcal}",
                $"{food.NutrientContent.Energy.KJ}",

                $"{food.NutrientContent.FatContent.Total}",
                $"{food.NutrientContent.FatContent.Total}",

                $"{food.NutrientContent.CarbohydrateContent.Total}",
                $"{food.NutrientContent.CarbohydrateContent.Sugar}",

                $"{food.NutrientContent.Protein.Total}",
                $"{food.NutrientContent.Salt.Total}",

                $"{food.Description}"
            };

            List<string> ingredientsInfo = new();
            try {
                ingredientsInfo = food.Ingredients.Select(ingredient => ingredient.Name).ToList();
            } catch { }

            string allIngredients = string.Join(",", ingredientsInfo);
            string foodInfo = $"Contains: {allIngredients}";

            if (allIngredients.Length == 0) {
                foodInfo = "Contains: Nothing else";
            }

            foodInfoList.Add(foodInfo);

            return foodInfoList;
        }

        /// <summary>
        /// Converts a list of strings into a <see cref="Food"/> object.
        /// </summary>
        /// <param name="list">A list of strings representing a food's properties.</param>
        /// <returns>A <see cref="Food"/> object initialized with the data from the string list.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided list does not contain at least 11 elements.</exception>
        public static Food FromStringList(List<string> list) {
            if (list == null || list.Count < 11) {
                throw new ArgumentException("List must contain at least 11 elements.");
            }

            Food food = new Food {
                Id = int.Parse(list[0]),
                Name = list[1],
                Weight = double.Parse(list[2]),
                NutrientContent = new Nutrients {
                    Energy = new Energy {
                        Kcal = double.Parse(list[3]),
                        KJ = double.Parse(list[4])
                    },
                    FatContent = new Fat {
                        Total = double.Parse(list[5]),
                        Saturated = double.Parse(list[6])
                    },
                    CarbohydrateContent = new Carbohydrates {
                        Total = double.Parse(list[7]),
                        Sugar = double.Parse(list[8])
                    },
                    Protein = new Protein {
                        Total = double.Parse(list[9])
                    },
                    Salt = new Salt {
                        Total = double.Parse(list[10])
                    }
                },
                Description = list[11]
            };

            return food;
        }

        /// <summary>
        /// Returns a string that represents the current food object in human-readable format.
        /// </summary>
        /// <returns>A string containing detailed information about the <see cref="Food"/> and its <see cref="Ingredients"/>.</returns>
        public override string ToString() {
            return ToReadableString(this);
        }

        /// <summary>
        /// Combines two <see cref="Food"/> objects by adding their <see cref="Food.Weight"/>, 
        /// <see cref="Food.NutrientContent"/>, and merging their <see cref="Food.Ingredients"/> lists. 
        /// The result is a new <see cref="Food"/> object with a default <see cref="Food.Id"/> of -1 
        /// and one of the original <see cref="Food.Name"/> values.
        /// </summary>
        /// <param name="food1">The first <see cref="Food"/> object.</param>
        /// <param name="food2">The second <see cref="Food"/> object.</param>
        /// <returns>A new <see cref="Food"/> object with combined weight, nutrients, and ingredients.</returns>
        public static Food operator +(Food food1, Food food2) {
            List<Food> ingredients = new();

            if (food1.Ingredients != null && food2.Ingredients != null) {
                ingredients = food1.Ingredients.Union(food2.Ingredients).ToList();
            } else if (food1.Ingredients != null) {
                ingredients = food1.Ingredients;
            } else if (food2.Ingredients != null) {
                ingredients = food2.Ingredients;
            }

            Food food = new Food {
                Id = -1,
                Name = food1.Name ?? food2.Name,
                Weight = food1.Weight + food2.Weight,
                NutrientContent = food1.NutrientContent + food2.NutrientContent,
                Description = "",
                Ingredients = ingredients
            };

            // if Foods that are combined into this Food are counted as ingredients already
            if (food.Ingredients.Count == 0) {
                food.AddIngredient(food1);
                food.AddIngredient(food2);
            }

            // Remove duplicates by this clever trick
            /*HashSet<Food> uniqueIngredients = new HashSet<Food>(food.Ingredients);
            List<Food> finalIngredients = uniqueIngredients.ToList();
            food.Ingredients = finalIngredients;*/

            return food;
        }

        /// <summary>
        /// Scales a <see cref="Food"/> object by multiplying its <see cref="Food.Weight"/>, 
        /// <see cref="Food.NutrientContent"/>, and scaling each item in the <see cref="Food.Ingredients"/> list 
        /// by a specified factor.
        /// </summary>
        /// <param name="factor">The factor by which to scale the food.</param>
        /// <param name="food">The <see cref="Food"/> object to be scaled.</param>
        /// <returns>A new <see cref="Food"/> object scaled by the factor.</returns>
        public static Food operator *(double factor, Food food) {
            // Scale the ingredients before returning scaled food
            List<Food> scaledIngredients =
                [.. food.Ingredients
                    .Select(ingredient => factor * ingredient)];

            return new Food {
                Id = food.Id,
                Name = food.Name,
                Weight = food.Weight * factor,
                NutrientContent = food.NutrientContent * factor,
                Ingredients = scaledIngredients,
                Description = food.Description
            };
        }

        /// <summary>
        /// Scales a <see cref="Food"/> object by multiplying its <see cref="Food.Weight"/>, 
        /// <see cref="Food.NutrientContent"/>, and scaling each item in the <see cref="Food.Ingredients"/> list 
        /// by a specified factor.
        /// </summary>
        /// <param name="factor">The factor by which to scale the food.</param>
        /// <param name="food">The <see cref="Food"/> object to be scaled.</param>
        /// <returns>A new <see cref="Food"/> object scaled by the factor.</returns>
        /// <remarks>Uses factor cast to double under the hood.</remarks>
        public static Food operator *(decimal factor, Food food) {
            return (double)factor * food;
        }

        /// <summary>
        /// Scales a <see cref="Food"/> object by multiplying its properties by a specified factor.
        /// This is a convenience method to support both factor-first and food-first multiplication.
        /// </summary>
        /// <param name="food">The <see cref="Food"/> object to be scaled.</param>
        /// <param name="factor">The factor by which to scale the food.</param>
        /// <returns>A new <see cref="Food"/> object scaled by the factor.</returns>
        public static Food operator *(Food food, double factor) {
            return factor * food;
        }

        /// <summary>
        /// Class representing favorite food to work with database with its options.
        /// </summary>
        public sealed class FavoriteFood {
            public Food Food { get; set; } = null!;
            public List<FavoriteFoodOption> Options { get; set; } = new();
        }

        /// <summary>
        /// Class representing option for favorite food to work with database.
        /// </summary>
        public sealed class FavoriteFoodOption {
            public decimal Weight { get; set; }
            public decimal? Price { get; set; }

            public FavoriteFoodOption() { }

            public FavoriteFoodOption(decimal weight, decimal? price = null) {
                Weight = weight;
                Price = price;
            }
        }
    }
}