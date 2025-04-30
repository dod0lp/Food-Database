using System.Numerics;
using System.Runtime.CompilerServices;
using Food_Database_Base;
using Microsoft.EntityFrameworkCore;

namespace Food
{
    // There is no check if some values are negative, for example negative amount of Protein, Fat...
    // If there is value not set, the default value in most, if not all, cases will be 0
    // for easier setting up of values, so we can't say when value was not set, but we can set
    // value of something to -1 to explicitly specify, that this value is not set yet

    /// <summary>
    /// Class describing Food - each food has its <see cref="Ingredients"/> - it is simply other <see cref="Food"/>
    /// </summary>
    /// <remarks>
    /// - Contains vague description of database. Database model is defined in <see cref="Food_Database_Base"/>
    /// </remarks>
    public class Food
    {
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
        private Food() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Food"/> class with specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the food.</param>
        /// <param name="name">The name of the food.</param>
        /// <param name="weight">The weight of the food in grams.</param>
        /// <param name="nutrientContent">The nutrient content of the food.</param>
        /// <param name="description">A description of the food.</param>
        public Food(int id, string name, double weight, Nutrients nutrientContent, string description)
        {
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
            : this(id, name, weight, nutrientContent, description)
        {
            Ingredients = ingredients;
        }

        /// <summary>
        /// Adds an ingredient to the list of <see cref="Ingredients"/> for this <see cref="Food"/>.
        /// </summary>
        /// <remarks>
        /// - The ingredient is simply other <see cref="Food"/>.
        /// </remarks>
        /// <param name="food">The food item to add as an ingredient.</param>
        public void AddIngredient(Food food)
        {
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
        public bool RemoveIngredient(int foodId)
        {
            var ingredient = Ingredients.FirstOrDefault(f => f.Id == foodId);

            if (ingredient != null)
            {
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
        public static string ToReadableString(Food food)
        {
            string foodInfo = $"ID: {food.Id}\nName: {food.Name}\nWeight: {food.Weight}\nNutrients: {food.NutrientContent}"
                +
                $"Description: {food.Description}\n";


            string foodsContained = "Contains: ";
            if (food.Ingredients != null)
            {
                foodsContained = string.Join("\n", food.Ingredients.Select(food => food.Name));
            }

            if (foodsContained == "Contains: ")
            {
                foodsContained = "";
            }

            return foodInfo + foodsContained;
        }

        /// <summary>
        /// Converts a food object into a list of strings representing its properties.
        /// </summary>
        /// <param name="food">The food object to convert.</param>
        /// <returns>A list of strings representing the <see cref="Food"/>'s properties and ingredients in human-readable format but in <see cref="List{Food}"/></returns>
        public static List<string> ToStringList(Food food)
        {
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
            try
            {
                ingredientsInfo = food.Ingredients.Select(ingredient => ingredient.Name).ToList();
            }
            catch { }

            string allIngredients = string.Join(",", ingredientsInfo);
            string foodInfo = $"Contains: {allIngredients}";

            if (allIngredients.Length == 0)
            {
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
        public static Food FromStringList(List<string> list)
        {
            if (list == null || list.Count < 11)
            {
                throw new ArgumentException("List must contain at least 11 elements.");
            }

            Food food = new Food
            {
                Id = int.Parse(list[0]),
                Name = list[1],
                Weight = double.Parse(list[2]),
                NutrientContent = new Nutrients
                {
                    Energy = new Energy
                    {
                        Kcal = double.Parse(list[3]),
                        KJ = double.Parse(list[4])
                    },
                    FatContent = new Fat
                    {
                        Total = double.Parse(list[5]),
                        Saturated = double.Parse(list[6])
                    },
                    CarbohydrateContent = new Carbohydrates
                    {
                        Total = double.Parse(list[7]),
                        Sugar = double.Parse(list[8])
                    },
                    Protein = new Protein
                    {
                        Total = double.Parse(list[9])
                    },
                    Salt = new Salt
                    {
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
        public override string ToString()
        {
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
        public static Food operator +(Food food1, Food food2)
        {
            List<Food> ingredients = new();

            if (food1.Ingredients != null && food2.Ingredients != null)
            {
                ingredients = food1.Ingredients.Union(food2.Ingredients).ToList();
            }
            else if (food1.Ingredients != null)
            {
                ingredients = food1.Ingredients;
            }
            else if (food2.Ingredients != null)
            {
                ingredients = food2.Ingredients;
            }

            Food food = new Food
            {
                Id = -1,
                Name = food1.Name ?? food2.Name,
                Weight = food1.Weight + food2.Weight,
                NutrientContent = food1.NutrientContent + food2.NutrientContent,
                Description = "",
                Ingredients = ingredients
            };

            // if Foods that are combined into this Food are counted as ingredients already
            if (food.Ingredients.Count == 0)
            {
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
        public static Food operator *(double factor, Food food)
        {
            // Scale the ingredients before returning scaled food
            List<Food> scaledIngredients = food.Ingredients
                .Select(ingredient => factor * ingredient)
                .ToList();

            return new Food
            {
                Id = food.Id,
                Name = food.Name,
                Weight = food.Weight * factor,
                NutrientContent = food.NutrientContent * factor,
                Ingredients = scaledIngredients,
                Description = food.Description
            };
        }

        /// <summary>
        /// Scales a <see cref="Food"/> object by multiplying its properties by a specified factor.
        /// This is a convenience method to support both factor-first and food-first multiplication.
        /// </summary>
        /// <param name="food">The <see cref="Food"/> object to be scaled.</param>
        /// <param name="factor">The factor by which to scale the food.</param>
        /// <returns>A new <see cref="Food"/> object scaled by the factor.</returns>
        public static Food operator *(Food food, double factor)
        {
            return factor * food;
        }
    }


    /// <summary>
    /// Provides methods for parsing and formatting <see cref="Food_Database_Base.FoodEntity"/> instances into human-readable strings.
    /// </summary>
    public static class FoodEntityStringParser
    {
        /// <summary>
        /// Converts a <see cref="Food_Database_Base.FoodEntity"/> instance into a human-readable string representation of food.
        /// </summary>
        /// <param name="foodEntity">The <see cref="Food_Database_Base.FoodEntity"/> instance to be formatted.</param>
        /// <returns>A string that presents the details of the <see cref="Food_Database_Base.FoodEntity"/> <paramref name="foodEntity"/> in a readable format,
        /// including its <see cref="FoodEntity.Name"/>, <see cref="FoodEntity.Weight"/>, <see cref="FoodEntity.Description"/>,
        /// <see cref="FoodEntity.Nutrient"/>, and <see cref="FoodEntity.IngredientsAsComplete"/>.</returns>
        public static string ParseEntityHumanReadable(Food_Database_Base.FoodEntity foodEntity)
        {
            string result = "";

            result += $"Name: {foodEntity.Name}\n";
            result += $"Weight: {foodEntity.Weight}g\n";
            result += $"Description: {foodEntity.Description}\n";

            if (foodEntity.Nutrient != null)
            {
                result += $"Nutritional Info:\n";
                result += $"  Energy: {foodEntity.Nutrient.EnergyKcal} kcal / {foodEntity.Nutrient.EnergyKj} kJ\n";
                result += $"  Total Fat: {foodEntity.Nutrient.FatTotal}g (Saturated: {foodEntity.Nutrient.FatSaturated}g)\n";
                result += $"  Total Carbs: {foodEntity.Nutrient.CarbsTotal}g (Sugar: {foodEntity.Nutrient.CarbsSaturated}g)\n";
                result += $"  Protein: {foodEntity.Nutrient.ProteinTotal}g\n";
                result += $"  Salt: {foodEntity.Nutrient.SaltTotal}g\n";
            }
            else
            {
                result += "No nutrient information available.\n";
            }

            // Print related ingredients, if there are none, it prints that there are none
            if (foodEntity.IngredientsAsComplete.Any())
            {
                result += "Ingredients:\n";
                foreach (var ingredient in foodEntity.IngredientsAsComplete)
                {
                    result += $"  Ingredient: {ingredient.FoodPart.Name}\n";
                }
            }
            else
            {
                result += "No ingredients listed.\n";
            }

            return result;
        }
    }


    public class Program_Food
    {
        public static void Main(string[] args)
        {
            //comparison operator for Food, Nutrients
            bool exampleNutrients = false;
            bool databaseExamples_Basics = false;
            bool databaseExamples_Scales_ReturnById = true;

            if (exampleNutrients)
            {
                Fat fat1 = new(10, 7);
                Console.WriteLine(fat1.Total);
                Console.WriteLine(fat1.Saturated);

                Energy en1 = new(100);
                Energy en2 = new(50);
                Console.WriteLine(en1 + en2);

                Nutrients nutrients1 = new(
                    new Energy(150),
                    new Fat(30, 15),
                    new Carbohydrates(70, 40),
                    new Protein(20),
                    new Salt(3.5)
                );

                Nutrients nutrients2 = new(
                    new Energy(100),
                    new Fat(20, 10),
                    new Carbohydrates(50, 30),
                    new Protein(15),
                    new Salt(2.5)
                );

                Nutrients resultAddNutrients = nutrients1 + nutrients2;
                Nutrients resultSubtractNutrients = nutrients1 - nutrients2;
                Nutrients resultScaleNutrients = 0.5 * nutrients1;

                Console.WriteLine(nutrients1);
                Console.WriteLine(nutrients2);

                Console.WriteLine($"Result of addition (Nutrients):\n{resultAddNutrients}");
                Console.WriteLine($"Result of subtraction (Nutrients):\n{resultSubtractNutrients}");
                Console.WriteLine($"Result of scaling (Nutrients):\n{resultScaleNutrients}");

                // Rounding up tests
                Fat fat2 = new (10.22222, 9.333);
                Console.WriteLine(fat2);
                Carbohydrates carbs1 = new(132.1321321, 25.43891);
                Console.WriteLine(carbs1);

                const double numberToRoundup = 10.123456789;
                Console.WriteLine($"\nRounding up {numberToRoundup}:");
                for (int i = 0; i < 9; i++)
                {
                    Console.Write($"{i} decimal places: ");
                    Console.WriteLine(NumberOperations.RoundUpToNDecimalPlaces(numberToRoundup, i));
                }


                Nutrients nutrientPotato = new(
                        new Energy(313),
                        new Fat(0.1, 0.02),
                        new Carbohydrates(23.2, 0.5),
                        new Protein(3.1),
                        new Salt(0.004)
                    );

                Food foodPotato = new(-1, "Potato", 100, nutrientPotato, "Root vegetable");

                FoodEntity foodPotatoEntity = foodPotato.MapToEntity();
                Food foodPotatoFromEntity = foodPotatoEntity.MapToDomain();
                Console.WriteLine(foodPotato);
                Console.WriteLine(foodPotatoFromEntity);

                NutrientEntity nutrientPotatoEntity = nutrientPotato.MapToEntity(foodPotato.Id);
                Nutrients nutrientPotatoFromEntity = nutrientPotatoEntity.MapToDomain();
                Console.WriteLine(nutrientPotato);
                Console.WriteLine(nutrientPotatoFromEntity);
            }

            if (databaseExamples_Basics)
            {
                using (var context = new FoodDbContext())
                {
                    DB_DataParser dbParser = new(context);
                    List<FoodEntity> foodsWithNutrients = dbParser.GetAllFoodEntity();

                    // Simply print all the retrieved food entities
                    foreach (FoodEntity foodEntity in foodsWithNutrients)
                    {
                        //Console.WriteLine(FoodEntityStringParser.ParseEntityHumanReadable(foodEntity));
                        // Console.WriteLine("-------------------");
                        // Console.WriteLine(foodEntity.MapToDomain().ToString());
                        var x = Food.ToStringList(foodEntity.MapToDomain());
                        foreach (var y in x)
                        {
                            Console.WriteLine(y);
                        }
                    }

                    Nutrients peasNutrients = new(
                        new Energy(81),
                        new Fat(0.4, 0.1),
                        new Carbohydrates(14, 6),
                        new Protein(5),
                        new Salt(0.05)
                    );

                    Food peasFood = new Food(-1, "Pea", 100, peasNutrients, "Green little balls.");
                    FoodEntity peasFoodEntity = peasFood.MapToEntity();
                    context.Foods.Add(peasFoodEntity);
                    context.SaveChanges();
                    int peasId = peasFoodEntity.FoodId;
                    var peasIngredients = new List<IngredientEntity>
                    {
                        new IngredientEntity
                        {
                            FoodIdComplete = peasId,
                            FoodIdPart = peasId
                        }
                    };
                    context.Ingredients.AddRange(peasIngredients);
                    context.SaveChanges();

                    Console.WriteLine("Food item added successfully.\n");

                    var peachEntity = new FoodEntity
                    {
                        Name = "Peach",
                        Weight = 150.0,
                        Description = "An orange fruit that is not an orange.",
                        Nutrient = new NutrientEntity
                        {
                            EnergyKcal = 200,
                            EnergyKj = 800,
                            FatTotal = 1.0,
                            FatSaturated = 2.0,
                            CarbsTotal = 30.0,
                            CarbsSaturated = 5.0,
                            ProteinTotal = 2.0,
                            SaltTotal = 0.2
                        }
                    };

                    context.Foods.Add(peachEntity);
                    context.SaveChanges();

                    int peachId = peachEntity.FoodId;

                    var peachIngredients = new List<IngredientEntity>
                    {
                        new IngredientEntity
                        {
                            FoodIdComplete = peachId,
                            FoodIdPart = peachId
                        }
                    };

                    context.Ingredients.AddRange(peachIngredients);
                    context.SaveChanges();

                    Console.WriteLine("Food item added successfully.\n");
                }
            }

            if (databaseExamples_Scales_ReturnById)
            {
                using (var context = new FoodDbContext())
                {
                    DB_DataParser dbParser = new(context);
                    List<FoodEntity> foodsWithNutrients = dbParser.GetAllFoodEntity();

                    Food food1 = foodsWithNutrients[59].MapToDomain();
                    Food food2 = foodsWithNutrients[60].MapToDomain();

                    Food food3 = food1 + food2;

                    food3.Name = "CombineFood1+2";
                    food3.Description = "Food 1 + Food 2.";

                    int newId1 = dbParser.InsertFoodFromDomain(food3);

                    /*int newId = dbParser.InsertFoodFromDomain(0.5 * food3);
                    dbParser.InsertFoodMappings(newId);
                    FoodEntity? newEntity = dbParser.GetFoodEntityById(newId);
                    if (newEntity != null)
                    {
                        Console.WriteLine(newEntity.MapToDomain());
                    }

                    Food? newFood = dbParser.GetFoodDomainModelById(newId);
                    if (newFood != null)
                    {
                        Console.WriteLine(newFood);
                    }*/

                    // dbParser.GetFoodPartIdsByCompleteFoodId(2);
                }
            }
        }
    }
} 