using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ProgramTestFood;

/// <summary>
/// Interactive commands for browsing foods and managing a selected user's data.
/// </summary>
public static class InteractiveMenu {
    /// <summary>
    /// Displays menu options until the user chooses to exit.
    /// </summary>
    public static async Task RunAsync(DB_FoodContext db, int userId) {
        while (true) {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("1 - Browse foods");
            Console.WriteLine("2 - Get food by ID");
            Console.WriteLine("3 - Create new food and favorite it");
            Console.WriteLine("4 - Favorite existing food");
            Console.WriteLine("5 - Show my favorites");
            Console.WriteLine("9 - Get food ingredient tree");
            Console.WriteLine("0 - Exit");
            Console.WriteLine("==============================================");
            Console.Write("> ");

            switch (Console.ReadLine()) {
                case "1":
                BrowseFoods(db);
                break;

                case "2":
                GetFoodById(db);
                break;

                case "3":
                CreateFoodAndFavorite(db, userId);
                break;

                case "4":
                await FavoriteExistingFoodAsync(db, userId);
                break;

                case "5":
                await ShowFavoritesWithOptionsAsync(db, userId);
                break;

                case "9":
                string res = await GetFoodIngredientTreeInputAsync(db) ?? "empty";
                Console.WriteLine(res);
                break;

                case "0":
                return;
            }
        }
    }

    /// <summary>
    /// Function to get foods from database and display their ID and Name.
    /// </summary>
    private static void BrowseFoods(DB_FoodContext db) {
        var foods = db.Food
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToList();

        Console.WriteLine();

        foreach (var food in foods) {
            Console.WriteLine($"{food.Id}: {food.Name}");
        }
    }

    /// <summary>
    /// Function to get food by ID from database and display its details.
    /// </summary>
    private static void GetFoodById(DB_FoodContext db) {
        Console.Write("Food ID: ");

        if (!int.TryParse(Console.ReadLine(), out int foodId)) {
            return;
        }

        Food_DBEntity? food = db.Food
            .AsNoTracking()
            .SingleOrDefault(x => x.Id == foodId);

        if (food is null) {
            Console.WriteLine("Food not found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"ID:            {food.Id}");
        Console.WriteLine($"Name:          {food.Name}");
        Console.WriteLine($"Description:   {food.Food_Description ?? "NULL"}");
        Console.WriteLine();
        Console.WriteLine("Nutrients per 100g:");
        Console.WriteLine($"Energy:        {InteractiveMenuHelpers.Format(food.Energy_Kcal, "kcal")}");
        Console.WriteLine($"Fat:           {InteractiveMenuHelpers.Format(food.Fat_Total, "g")}");
        Console.WriteLine($"Saturated fat: {InteractiveMenuHelpers.Format(food.Fat_Saturated, "g")}");
        Console.WriteLine($"Carbohydrates: {InteractiveMenuHelpers.Format(food.Carbs_Total, "g")}");
        Console.WriteLine($"Sugar:         {InteractiveMenuHelpers.Format(food.Carbs_Sugar, "g")}");
        Console.WriteLine($"Protein:       {InteractiveMenuHelpers.Format(food.Protein_Total, "g")}");
        Console.WriteLine($"Salt:          {InteractiveMenuHelpers.Format(food.Salt_Total, "g")}");
    }

    /// <summary>
    /// Function to test creating a new food and adding it to the user's favorites.
    /// </summary>
    private static void CreateFoodAndFavorite(
        DB_FoodContext db,
        int userId) {
        Users_DBEntity? user = db.Users
            .Include(x => x.Food)
            .SingleOrDefault(x => x.Id == userId);

        if (user is null) {
            return;
        }

        Console.Write("Food name: ");
        string? name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name)) {
            return;
        }

        Console.Write("Energy kcal / 100g: ");
        int.TryParse(Console.ReadLine(), out int kcal);

        Console.Write("Fat / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal fat);

        Console.Write("Saturated fat / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal saturated);

        Console.Write("Carbs / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal carbs);

        Console.Write("Sugar / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal sugar);

        Console.Write("Protein / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal protein);

        Console.Write("Salt / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal salt);

        var newFood = new Food_DBEntity {
            Name = name,
            Energy_Kcal = kcal,
            Fat_Total = fat,
            Fat_Saturated = saturated,
            Carbs_Total = carbs,
            Carbs_Sugar = sugar,
            Protein_Total = protein,
            Salt_Total = salt
        };

        // First create the actual Food row so SQL generates its ID.
        db.Food.Add(newFood);
        db.SaveChanges();

        // Mark this food as created by this user.
        // this 100% can be done through entity.UserCreatedFood and just add id of user
        db.UserCreatedFood.Add(new UserCreatedFood_DBEntity {
            Food_Id = newFood.Id,
            User_Id = userId
        });

        // make it this user's favorite.
        user.Food.Add(newFood);

        db.SaveChanges();

        Console.WriteLine(
            $"Created food {newFood.Id}: {newFood.Name} and added to favorites.");
    }

    /// <summary>
    /// Function to favorite an existing food by ID for the user.
    /// </summary>
    private static async Task FavoriteExistingFoodAsync(
        DB_FoodContext db,
        int userId) {
        BrowseFoods(db);

        Console.WriteLine();
        Console.Write("Food ID to favorite: ");

        if (!int.TryParse(Console.ReadLine(), out int foodId)) {
            return;
        }

        Users_DBEntity? user = await db.Users
            .Include(x => x.Food)
            .SingleOrDefaultAsync(x => x.Id == userId);

        if (user is null) {
            return;
        }

        Food_DBEntity? food = await db.Food
            .SingleOrDefaultAsync(x => x.Id == foodId);

        if (food is null) {
            Console.WriteLine("Food not found.");
            return;
        }

        if (user.Food.Any(x => x.Id == foodId)) {
            Console.WriteLine("Food is already a favorite.");
            return;
        }

        user.Food.Add(food);

        await db.SaveChangesAsync();

        Console.WriteLine($"Added {food.Id}: {food.Name} to favorites.");
    }

    /// <summary>
    /// Function to show the user's favorite foods with their options.
    /// </summary>
    private static async Task ShowFavoritesWithOptionsAsync(
    DB_FoodContext db,
    int userId) {
        var favorites = await db.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .SelectMany(x => x.Food)
            .Select(food => new {
                Food = food,

                Options = food.UserFoodOptions
                    .Where(x => x.User_Id == userId)
                    .ToList()
            })
            .OrderBy(x => x.Food.Id)
            .ToListAsync();

        Console.WriteLine();

        if (favorites.IsNullOrEmpty()) {
            Console.WriteLine("No favorites.");
            return;
        }

        foreach (var item in favorites) {
            Console.WriteLine(
                $"{item.Food.Id}: {item.Food.Name}");

            foreach (var option in item.Options) {
                Console.WriteLine(
                    $"  Weight: {(option != null
                        ? $"{option.Weight_Total:0.00} g"
                        : "not set")}");

                Console.WriteLine(
                    $"  Price:  {(option?.Price_Eur.HasValue == true
                        ? $"{option.Price_Eur.Value:0.00} EUR"
                        : "not set")}");
            }
        }
    }

    /// <summary>
    /// Prompts for a food ID and returns its formatted ingredient tree.
    /// </summary>
    public static async Task<string?> GetFoodIngredientTreeInputAsync(DB_FoodContext db) {
        Console.Write("Food ID: ");

        if (!int.TryParse(Console.ReadLine(), out int foodId)) {
            Console.Write("oh no");
            return null;
        }

        string res = await InteractiveMenuHelpers.GetFoodIngredientTreeAsync(db, foodId);
        return res;
    }


}