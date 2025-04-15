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
    /// Class for specific operations with numbers.
    /// </summary>
    public static class NumberOperations
    {
        public static int[] lookup_powers_10 = new int[9];

        static NumberOperations()
        {
            for (int i = 0; i < lookup_powers_10.Length; i++)
            {
                lookup_powers_10[i] = (int)Math.Pow(10, i);
            }
        }

        /// <summary>
        /// Round up double to <see cref="N"/> decimal places.<br></br>
        /// </summary>
        /// <param name="number">Number to be rounded up.</param>
        /// <param name="N">Number of total decimal places.</param>
        /// <returns></returns>
        public static double RoundUpToNDecimalPlaces(double number, int N)
        {
            return Math.Ceiling(number * lookup_powers_10[N]) / lookup_powers_10[N];
        }

        /// <summary>
        /// Specialized class for rounding up to 2decimal places.
        /// </summary>
        /// <param name="number">Number to be rounded up.</param>
        /// <returns></returns>
        public static double RoundUpTo2DecimalPlaces(double number)
        {
            // return RoundUpToNDecimalPlaces(number, 2);
            return Math.Ceiling(number * 100) / 100;
        }
    }

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
    /// <see cref="Nutrients"/> content of certain <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>
    /// <br></br>
    /// Represents nutritional information including <see cref="Energy"/>, <see cref="FatContent"/>, <see cref="CarbohydrateContent"/>, <see cref="Protein"/>, and <see cref="Salt"/> content.
    /// </summary>
    public struct Nutrients
    {
        /// <summary>
        /// Gets or sets the energy content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>.
        /// </summary>
        public Energy Energy { get; set; }

        /// <summary>
        /// Gets or sets the fat content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>..
        /// </summary>
        public Fat FatContent { get; set; }

        /// <summary>
        /// Gets or sets the carbohydrate content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>..
        /// </summary>
        public Carbohydrates CarbohydrateContent { get; set; }

        /// <summary>
        /// Gets or sets the protein content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>..
        /// </summary>
        public Protein Protein { get; set; }

        /// <summary>
        /// Gets or sets the salt content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>..
        /// </summary>
        public Salt Salt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nutrients"/> struct with specified nutritional values.
        /// </summary>
        /// <param name="energy">The energy content.</param>
        /// <param name="fatContent">The fat content.</param>
        /// <param name="carbohydrateContent">The carbohydrate content.</param>
        /// <param name="protein">The protein content.</param>
        /// <param name="salt">The salt content.</param>
        public Nutrients(Energy energy, Fat fatContent, Carbohydrates carbohydrateContent, Protein protein, Salt salt)
        {
            Energy = energy;
            FatContent = fatContent;
            CarbohydrateContent = carbohydrateContent;
            Protein = protein;
            Salt = salt;
        }

        /// <summary>
        /// Adds two instances of <see cref="Nutrients"/>, combining their respective nutritional values.
        /// </summary>
        /// <param name="n1">The first <see cref="Nutrients"/> instance.</param>
        /// <param name="n2">The second <see cref="Nutrients"/> instance.</param>
        /// <returns>A new <see cref="Nutrients"/> instance with summed nutritional values.</returns>
        public static Nutrients operator +(Nutrients n1, Nutrients n2)
        {
            return new Nutrients(
                n1.Energy + n2.Energy,
                n1.FatContent + n2.FatContent,
                n1.CarbohydrateContent + n2.CarbohydrateContent,
                n1.Protein + n2.Protein,
                n1.Salt + n2.Salt
            );
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Nutrients"/> from another, subtracting their respective nutritional values.
        /// </summary>
        /// <param name="n1">The first <see cref="Nutrients"/> instance.</param>
        /// <param name="n2">The second <see cref="Nutrients"/> instance.</param>
        /// <returns>A new <see cref="Nutrients"/> instance with subtracted nutritional values.</returns>
        public static Nutrients operator -(Nutrients n1, Nutrients n2)
        {
            return new Nutrients(
                n1.Energy - n2.Energy,
                n1.FatContent - n2.FatContent,
                n1.CarbohydrateContent - n2.CarbohydrateContent,
                n1.Protein - n2.Protein,
                n1.Salt - n2.Salt
            );
        }

        /// <summary>
        /// Scales the nutritional values of a <see cref="Nutrients"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="nutrients">The <see cref="Nutrients"/> instance to scale.</param>
        /// <returns>A new <see cref="Nutrients"/> instance with scaled nutritional values.</returns>
        public static Nutrients operator *(double factor, Nutrients nutrients)
        {
            return new Nutrients(
                factor * nutrients.Energy,
                factor * nutrients.FatContent,
                factor * nutrients.CarbohydrateContent,
                factor * nutrients.Protein,
                factor * nutrients.Salt
            );
        }

        /// <summary>
        /// Scales the nutritional values of a <see cref="Nutrients"/> instance by a specified factor.
        /// </summary>
        /// <param name="nutrients">The <see cref="Nutrients"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Nutrients"/> instance with scaled nutritional values.</returns>
        public static Nutrients operator *(Nutrients nutrients, double factor)
        {
            return factor * nutrients;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Nutrients"/> instance, displaying its nutritional values.
        /// </summary>
        /// <returns>A string representation of the <see cref="Nutrients"/> instance.</returns>
        public override readonly string ToString()
        {
            return $"Energy: {Energy}\nFat: {FatContent}\nCarbohydrates: {CarbohydrateContent}\nProtein: {Protein}\nSalt: {Salt}\n";
        }
    }

    /// <summary>
    /// Represents energy information in both <see cref="kcal"/> (kilocalories) and <see cref="kj"/> (kilojoules).
    /// </summary>
    public struct Energy
    {
        private static readonly double KcalToKjFactor = 4.184F;

        private int kcal;
        private int kj;

        /// <summary>
        /// Gets or sets the energy value in <see cref="kcal"/> (kilocalories).
        /// </summary>
        public double Kcal
        {
            readonly get => kcal;

            set
            {
                kcal = (int)Math.Ceiling(value);
                kj = (int)Math.Ceiling(value * KcalToKjFactor);
            }
        }

        /// <summary>
        /// Gets or sets the energy value in <see cref="kj"/> (kilojoules).
        /// </summary>
        public double KJ
        {
            readonly get => kj;

            set
            {
                kj = (int)Math.Ceiling(value);
                kcal = (int)Math.Ceiling(value / KcalToKjFactor);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Energy"/> struct with a specified energy value.<br></br>
        /// Can initialize with either <see cref="Kcal"/> or <see cref="KJ"/>
        /// </summary>
        /// <param name="value">The energy value.</param>
        /// <param name="isKcal">Specify true if the provided value is in kcal (default), false if in kJ.</param>
        public Energy(double value, bool isKcal = true)
        {
            if (isKcal)
            {
                Kcal = value;
            }
            else
            {
                KJ = value;
            }
        }

        /// <summary>
        /// Adds two instances of <see cref="Energy"/>, combining their kcal values.
        /// </summary>
        /// <param name="e1">The first <see cref="Energy"/> instance.</param>
        /// <param name="e2">The second <see cref="Energy"/> instance.</param>
        /// <returns>A new <see cref="Energy"/> instance with summed energy values in kcal.</returns>
        public static Energy operator +(Energy e1, Energy e2)
        {
            return new Energy(e1.Kcal + e2.Kcal);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Carbohydrates"/> from another, subtracting their <see cref="Carbohydrates.Total"/> and <see cref="Carbohydrates.Sugar"/> amounts.
        /// <br></br><br ></br>
        /// In order for values to not become negative, you need to ensure that you are subtracting <see cref="Food.NutrientContent"/>.<see cref="Carbohydrates"/>
        /// <br></br>
        /// contained in other <see cref="Food.NutrientContent"/>.<see cref="Carbohydrates"/>
        /// <br></br>
        /// </summary>
        /// <param name="e1">The first <see cref="Energy"/> instance.</param>
        /// <param name="e2">The second <see cref="Energy"/> instance.</param>
        /// <returns>A new <see cref="Energy"/> instance with subtracted energy values in kcal.</returns>
        public static Energy operator -(Energy e1, Energy e2)
        {
            return new Energy(e1.Kcal - e2.Kcal);
        }

        /// <summary>
        /// Scales the energy value of a <see cref="Energy"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="e">The <see cref="Energy"/> instance to scale.</param>
        /// <returns>A new <see cref="Energy"/> instance with scaled energy values in kcal.</returns>
        public static Energy operator *(double factor, Energy e)
        {
            return new Energy(e.Kcal * factor);
        }

        /// <summary>
        /// Scales the energy value of a <see cref="Energy"/> instance by a specified factor.
        /// </summary>
        /// <param name="e">The <see cref="Energy"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Energy"/> instance with scaled energy values in kcal.</returns>
        public static Energy operator *(Energy e, double factor)
        {
            return factor * e;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Energy"/> instance, displaying its energy values in kcal and kJ.
        /// </summary>
        /// <returns>A string representation of the <see cref="Energy"/> instance.</returns>
        public override readonly string ToString()
        {
            return $"{Kcal} kcal ({KJ} kJ)";
        }
    }

    /// <summary>
    /// Represents a nutrient with a specific total amount. 
    /// Supports various arithmetic operations such as addition, subtraction, and scaling through overloaded operators.
    /// </summary>
    /// <remarks>
    /// The <see cref="Nutrient"/> class allows users to perform the following operations:
    /// <list type="bullet">
    ///     <item><description>Addition (+): Combines the total amounts of two <see cref="Nutrient"/> instances.</description></item>
    ///     <item><description>Subtraction (-): Subtracts the total amount of one <see cref="Nutrient"/> from another.</description></item>
    ///     <item><description>Multiplication (*): Scales a <see cref="Nutrient"/> instance by a specified factor.</description></item>
    /// </list>
    /// This class ensures that the total amount of the nutrient is always rounded to two decimal places when initialized or modified.
    /// </remarks>
    public class Nutrient
    {
        /// <summary>
        /// Gets or sets the total amount of <see cref="Nutrient"/> in grams.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nutrient"/> class with specified values.
        /// </summary>
        /// <param name="total">The total amount of nutrient in grams (default is 0).</param>
        public Nutrient(double total = 0)
        {
            Total = NumberOperations.RoundUpTo2DecimalPlaces(total);
        }

        /// <summary>
        /// String for setting how total value of <see cref="Nutrient"/> is displayed
        /// </summary>
        /// <param name="total">Simply put <see cref="Total"/> here.</param>
        /// <returns></returns>
        protected static string Str_Total(double total)
        {
            return $"Total: {total}";
        }

        /// <summary>
        /// Adds two instances of <see cref="Nutrient"/>, combining their <see cref="Nutrient.Total"/> amounts.
        /// </summary>
        /// <param name="n1">The first <see cref="Nutrient"/> instance.</param>
        /// <param name="n2">The second <see cref="Nutrient"/> instance.</param>
        /// <returns>A new <see cref="Nutrient"/> instance with summed protein values.</returns>
        public static Nutrient operator +(Nutrient n1, Nutrient n2)
        {
            return new Nutrient(n1.Total + n2.Total);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Nutrient"/> from another, subtracting their <see cref="Nutrient.Total"/> amounts.
        /// </summary>
        /// <param name="n1">The first <see cref="Nutrient"/> instance.</param>
        /// <param name="n2">The second <see cref="Nutrient"/> instance.</param>
        /// <returns>A new <see cref="Nutrient"/> instance with subtracted nutrient values.</returns>
        public static Nutrient operator -(Nutrient n1, Nutrient n2)
        {
            return new Nutrient(n1.Total - n2.Total);
        }

        /// <summary>
        /// Scales the nutrient value of a <see cref="Nutrient"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="n">The <see cref="Nutrient"/> instance to scale.</param>
        /// <returns>A new <see cref="Nutrient"/> instance with scaled <see cref="Nutrient.Total"/> value.</returns>
        public static Nutrient operator *(double factor, Nutrient n)
        {
            return new Nutrient(n.Total * factor);
        }

        /// <summary>
        /// Scales the nutrient value of a <see cref="Nutrient"/> instance by a specified factor.
        /// </summary>
        /// <param name="n">The <see cref="Nutrient"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Nutrient"/> instance with scaled <see cref="Nutrient.Total"/> value.</returns>
        public static Nutrient operator *(Nutrient n, double factor)
        {
            return factor * n;
        }

        public override string ToString()
        {
            return $"{Str_Total(Total)}";
        }
    }

    /// <summary>
    /// Represents nutritional information related to <see cref="Fat"/>.
    /// </summary>
    public class Fat : Nutrient
    {
        /// <summary>
        /// Gets or sets the amount of saturated fat in grams.
        /// </summary>
        public double Saturated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fat"/> struct with specified values.
        /// </summary>
        /// <param name="total">The total amount of fat in grams (default is 0).</param>
        /// <param name="saturated">The amount of saturated fat in grams (default is 0).</param>
        public Fat(double total = 0, double saturated = 0) : base(total)
        {
            Saturated = NumberOperations.RoundUpTo2DecimalPlaces(saturated);
        }

        /// <summary>
        /// Adds two instances of <see cref="Fat"/>, combining their total and saturated <see cref="Fat"/> amounts.
        /// </summary>
        /// <param name="f1">The first <see cref="Fat"/> instance.</param>
        /// <param name="f2">The second <see cref="Fat"/> instance.</param>
        /// <returns>A new <see cref="Fat"/> instance with summed <see cref="Fat"/> values.</returns>
        public static Fat operator +(Fat f1, Fat f2)
        {
            return new Fat(f1.Total + f2.Total, f1.Saturated + f2.Saturated);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Fat"/> from another, subtracting their <see cref="Fat.Total"/> and <see cref="Fat.Saturated"/> amounts.
        /// <br></br><br></br>
        /// In order for values to not become negative, you need to ensure that you are subtracting <see cref="Food.NutrientContent"/>.<see cref="Fat"/>
        /// <br></br>
        /// contained in other <see cref="Food.NutrientContent"/>.<see cref="Fat"/>
        /// <br></br>
        /// </summary>
        /// <param name="f1">The first <see cref="Fat"/> instance.</param>
        /// <param name="f2">The second <see cref="Fat"/> instance.</param>
        /// <returns>A new <see cref="Fat"/> instance with subtracted <see cref="Fat"/> values.</returns>
        public static Fat operator -(Fat f1, Fat f2)
        {
            return new Fat(f1.Total - f2.Total, f1.Saturated - f2.Saturated);
        }

        /// <summary>
        /// Scales the fat values of a <see cref="Fat"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="f">The <see cref="Fat"/> instance to scale.</param>
        /// <returns>A new <see cref="Fat"/> instance with scaled <see cref="Fat"/> values.</returns>
        public static Fat operator *(double factor, Fat f)
        {
            return new Fat(f.Total * factor, f.Saturated * factor);
        }

        /// <summary>
        /// Scales the fat values of a <see cref="Fat"/> instance by a specified factor.
        /// </summary>
        /// <param name="f">The <see cref="Fat"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Fat"/> instance with scaled <see cref="Fat"/> values.</returns>
        public static Fat operator *(Fat f, double factor)
        {
            return factor * f;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Fat"/> instance, displaying its <see cref="Fat.Total"/> and <see cref="Fat.Saturated"/> amounts.
        /// </summary>
        /// <returns>A string representation of the <see cref="Fat"/> instance.</returns>
        public override string ToString()
        {
            return $"{Str_Total(Total)}, Saturated: {Saturated}";
        }
    }

    /// <summary>
    /// Represents nutritional information related to <see cref="Carbohydrates"/>.
    /// </summary>
    public class Carbohydrates : Nutrient
    {
        /// <summary>
        /// Gets or sets the amount of sugar in grams.
        /// </summary>
        public double Sugar { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Carbohydrates"/> struct with specified values.
        /// </summary>
        /// <param name="total">The total amount of carbohydrates in grams (default is 0).</param>
        /// <param name="sugar">The amount of sugar in grams (default is 0).</param>
        public Carbohydrates(double total = 0, double sugar = 0) : base(total)
        {
            Sugar = NumberOperations.RoundUpTo2DecimalPlaces(sugar);
        }

        /// <summary>
        /// Adds two instances of <see cref="Carbohydrates"/>, combining their <see cref="Carbohydrates.Total"/> and <see cref="Carbohydrates.Sugar"/> amounts.
        /// </summary>
        /// <param name="c1">The first <see cref="Carbohydrates"/> instance.</param>
        /// <param name="c2">The second <see cref="Carbohydrates"/> instance.</param>
        /// <returns>A new <see cref="Carbohydrates"/> instance with summed carbohydrate values.</returns>
        public static Carbohydrates operator +(Carbohydrates c1, Carbohydrates c2)
        {
            return new Carbohydrates(c1.Total + c2.Total, c1.Sugar + c2.Sugar);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Carbohydrates"/> from another, subtracting their <see cref="Carbohydrates.Total"/> and <see cref="Carbohydrates.Sugar"/> amounts.
        /// <br></br><br ></br>
        /// In order for values to not become negative, you need to ensure that you are subtracting <see cref="Food.NutrientContent"/>.<see cref="Carbohydrates"/>
        /// <br></br>
        /// contained in other <see cref="Food.NutrientContent"/>.<see cref="Carbohydrates"/>
        /// <br></br>
        /// </summary>
        /// <param name="c1">The first <see cref="Carbohydrates"/> instance.</param>
        /// <param name="c2">The second <see cref="Carbohydrates"/> instance.</param>
        /// <returns>A new <see cref="Carbohydrates"/> instance with subtracted <see cref="Carbohydrates"/> values.</returns>
        public static Carbohydrates operator -(Carbohydrates c1, Carbohydrates c2)
        {
            return new Carbohydrates(c1.Total - c2.Total, c1.Sugar - c2.Sugar);
        }

        /// <summary>
        /// Scales the carbohydrate values of a <see cref="Carbohydrates"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="c">The <see cref="Carbohydrates"/> instance to scale.</param>
        /// <returns>A new <see cref="Carbohydrates"/> instance with scaled carbohydrate values.</returns>
        public static Carbohydrates operator *(double factor, Carbohydrates c)
        {
            return new Carbohydrates(c.Total * factor, c.Sugar * factor);
        }

        /// <summary>
        /// Scales the carbohydrate values of a <see cref="Carbohydrates"/> instance by a specified factor.
        /// </summary>
        /// <param name="c">The <see cref="Carbohydrates"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Carbohydrates"/> instance with scaled carbohydrate values.</returns>
        public static Carbohydrates operator *(Carbohydrates c, double factor)
        {
            return factor * c;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Carbohydrates"/> instance, displaying its <see cref="Carbohydrates.Total"/> and <see cref="Carbohydrates.Sugar"/> amounts.
        /// </summary>
        /// <returns>A string representation of the <see cref="Carbohydrates"/> instance.</returns>
        public override string ToString()
        {
            return $"{Str_Total(Total)}, Sugar: {Sugar}";
        }
    }

    /// <summary>
    /// Represents nutritional information related to <see cref="Protein"/>.
    /// </summary>
    public class Protein : Nutrient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Protein"/> class with specified values.
        /// </summary>
        /// <param name="total">The total amount of protein in grams (default is 0).</param>
        public Protein(double total = 0) : base(total) { }

        /// <summary>
        /// Adds two instances of <see cref="Protein"/>, combining their <see cref="Protein.Total"/> amounts.
        /// </summary>
        /// <param name="p1">The first <see cref="Protein"/> instance.</param>
        /// <param name="p2">The second <see cref="Protein"/> instance.</param>
        /// <returns>A new <see cref="Protein"/> instance with summed protein values.</returns>
        public static Protein operator +(Protein p1, Protein p2)
        {
            // return new Protein(((Nutrient)p1 + (Nutrient)p2).Total);
            return new Protein(p1.Total + p2.Total);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Protein"/> from another, subtracting their <see cref="Protein.Total"/> amounts.
        /// </summary>
        /// <param name="p1">The first <see cref="Protein"/> instance.</param>
        /// <param name="p2">The second <see cref="Protein"/> instance.</param>
        /// <returns>A new <see cref="Protein"/> instance with subtracted protein values.</returns>
        public static Protein operator -(Protein p1, Protein p2)
        {
            return new Protein(p1.Total - p2.Total);
        }

        /// <summary>
        /// Scales the protein value of a <see cref="Protein"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="p">The <see cref="Protein"/> instance to scale.</param>
        /// <returns>A new <see cref="Protein"/> instance with scaled <see cref="Protein.Total"/> value.</returns>
        public static Protein operator *(double factor, Protein p)
        {
            return new Protein(p.Total * factor);
        }

        /// <summary>
        /// Scales the protein value of a <see cref="Protein"/> instance by a specified factor.
        /// </summary>
        /// <param name="p">The <see cref="Protein"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Protein"/> instance with scaled <see cref="Protein.Total"/> value.</returns>
        public static Protein operator *(Protein p, double factor)
        {
            return factor * p;
        }
    }

    /// <summary>
    /// Represents nutritional information related to <see cref="Salt"/>.
    /// </summary>
    public class Salt : Nutrient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Salt"/> class with specified values.
        /// </summary>
        /// <param name="total">The total amount of salt in grams (default is 0).</param>
        public Salt(double total = 0) : base(total) { }

        /// <summary>
        /// Adds two instances of <see cref="Salt"/>, combining their <see cref="Salt.Total"/> amounts.
        /// </summary>
        /// <param name="s1">The first <see cref="Salt"/> instance.</param>
        /// <param name="s2">The second <see cref="Salt"/> instance.</param>
        /// <returns>A new <see cref="Salt"/> instance with summed salt values.</returns>
        public static Salt operator +(Salt s1, Salt s2)
        {
            return new Salt(s1.Total + s2.Total);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Salt"/> from another, subtracting their <see cref="Salt.Total"/> amounts.
        /// </summary>
        /// <param name="s1">The first <see cref="Salt"/> instance.</param>
        /// <param name="s2">The second <see cref="Salt"/> instance.</param>
        /// <returns>A new <see cref="Salt"/> instance with subtracted protein values.</returns>
        public static Salt operator -(Salt s1, Salt s2)
        {
            return new Salt(s1.Total - s2.Total);
        }

        /// <summary>
        /// Scales the salt value of a <see cref="Salt"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="s">The <see cref="Salt"/> instance to scale.</param>
        /// <returns>A new <see cref="Salt"/> instance with scaled <see cref="Salt.Total"/> value.</returns>
        public static Salt operator *(double factor, Salt s)
        {
            return new Salt(s.Total * factor);
        }

        /// <summary>
        /// Scales the salt value of a <see cref="Salt"/> instance by a specified factor.
        /// </summary>
        /// <param name="s">The <see cref="Salt"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Salt"/> instance with scaled <see cref="Salt.Total"/> value.</returns>
        public static Salt operator *(Salt s, double factor)
        {
            return factor * s;
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