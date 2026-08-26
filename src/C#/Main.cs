using CsvHelper;
using Food_Database.Database.Descriptors;
using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static Food.Food;
using static Food_Database.Database.Operations.EFLoader;
using FoodParser;
using Microsoft.IdentityModel.Tokens;

public static class Program_Food {
    private const int UserId = 2;
    private const int FoodId = 50;

    static async Task Main() {
        var options = new DbContextOptionsBuilder<DB_FoodContext>()
            .UseSqlServer(
                DB_Food_Descriptors.ConnectionString,
                sqlOptions => sqlOptions.EnableRetryOnFailure())
            .Options;

        using var db = new DB_FoodContext(options);

        await EnsureDummyData(db);
        Users_DBEntity? user = await db.Users
            .Include(x => x.Food)
            .SingleOrDefaultAsync(
                x => x.Id == UserId);

        if (user is null) {
            Console.WriteLine("UserID doesn't exist in database. Exiting.");
            return;
        }

        while (true) {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("1 - Browse foods");
            Console.WriteLine("2 - Get food by ID");
            Console.WriteLine("3 - Create new food and favorite it");
            Console.WriteLine("4 - Favorite existing food");
            Console.WriteLine("5 - Show my favorites");
            Console.WriteLine("6 - Test favorite food options");
            Console.WriteLine("7 - Create new food from existing food");
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
                CreateFoodAndFavorite(db, UserId);
                break;

                case "4":
                FavoriteExistingFood(db, UserId);
                break;

                case "5":
                ShowFavoritesWithOptions(db, UserId);
                break;

                case "6":
                await TestFavoriteFoodOptions(db, UserId, FoodId);
                break;

                case "7":
                await TryCreateFoodFromExisting(db, UserId);
                break;

                case "0":
                return;
            }
        }
    }

    private static async Task EnsureDummyData(DB_FoodContext db) {
        const int userCountWanted = 10;
        const int foodCountWanted = 50;

        if (await db.Users.Take(userCountWanted).CountAsync() < userCountWanted) {
            List<Users_DBEntity> users = [..
                Enumerable.Range(0, userCountWanted)
                    .Select(_ => new Users_DBEntity())
            ];

            db.Users.AddRange(users);
        }

        if (await db.Food.Take(foodCountWanted).CountAsync() < foodCountWanted) {
            await FoodCsvMap.ParseCsvIntoDB(db);
        }

        await db.SaveChangesAsync();
    }

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
        Console.WriteLine($"Energy:        {Format(food.Energy_Kcal, "kcal")}");
        Console.WriteLine($"Fat:           {Format(food.Fat_Total, "g")}");
        Console.WriteLine($"Saturated fat: {Format(food.Fat_Saturated, "g")}");
        Console.WriteLine($"Carbohydrates: {Format(food.Carbs_Total, "g")}");
        Console.WriteLine($"Sugar:         {Format(food.Carbs_Sugar, "g")}");
        Console.WriteLine($"Protein:       {Format(food.Protein_Total, "g")}");
        Console.WriteLine($"Salt:          {Format(food.Salt_Total, "g")}");
    }

    private static string Format(int? value, string unit) {
        return value.HasValue
            ? $"{value.Value} {unit}"
            : "-1";
    }

    private static string Format(decimal? value, string unit) {
        return value.HasValue
            ? $"{value.Value} {unit}"
            : "-1";
    }

    private static void CreateFoodAndFavorite(
        DB_FoodContext db,
        int userId) {
        Users_DBEntity? user = db.Users
            .Include(x => x.Food)
            .SingleOrDefault(x => x.Id == userId);

        if (user is null)
            return;

        Console.Write("Food name: ");
        string? name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
            return;

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
        db.UserCreatedFood.Add(new UserCreatedFood_DBEntity {
            Food_Id = newFood.Id,
            User_Id = userId
        });

        // Also make it this user's favorite.
        user.Food.Add(newFood);

        db.SaveChanges();

        Console.WriteLine(
            $"Created food {newFood.Id}: {newFood.Name} and added to favorites.");
    }

    private static void FavoriteExistingFood(
        DB_FoodContext db,
        int userId) {
        BrowseFoods(db);

        Console.WriteLine();
        Console.Write("Food ID to favorite: ");

        if (!int.TryParse(Console.ReadLine(), out int foodId))
            return;

        Users_DBEntity? user = db.Users
            .Include(x => x.Food)
            .SingleOrDefault(x => x.Id == userId);

        if (user is null)
            return;

        Food_DBEntity? food = db.Food
            .SingleOrDefault(x => x.Id == foodId);

        if (food is null) {
            Console.WriteLine("Food not found.");
            return;
        }

        if (user.Food.Any(x => x.Id == foodId)) {
            Console.WriteLine("Food is already a favorite.");
            return;
        }

        user.Food.Add(food);

        db.SaveChanges();

        Console.WriteLine(
            $"Added {food.Id}: {food.Name} to favorites.");
    }

    private static void ShowFavoritesWithOptions(
    DB_FoodContext db,
    int userId) {
        var favorites = db.Users
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
            .ToList();

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

    private static async Task TestFavoriteFoodOptions(DB_FoodContext db, int userId, int foodId) {
        var repo = new FoodRepository(db);
        var ct = CancellationToken.None; // default is none so it's w.e. if used here

        await repo.AddFavoriteFoodOptionAsync(userId, foodId, new(100, 19), ct);
        await repo.AddFavoriteFoodOptionAsync(userId, foodId, new(120, 29), ct);
        await repo.AddFavoriteFoodOptionAsync(userId, foodId, new(130, 39), ct);
        await repo.AddFavoriteFoodOptionAsync(userId, foodId, new(140, 59), ct);

        await repo.SaveChangesDBAsync();

        ShowFavoritesWithOptions(db, userId);
    }

    private static async Task TryCreateFoodFromExisting(DB_FoodContext db, int userId) {
        var repo = new FoodRepository(db);

        Dictionary<int, decimal> ingredientWeights = Enumerable
            .Range(0, Random.Shared.Next(2, 5)) // 2-4ingr
                .ToDictionary(
                    _ => Random.Shared.Next(1, 251), // ID
                    _ => Math.Round((decimal)(Random.Shared.NextDouble() * 245 + 5), 2) // amount 5-250
        );

        Food.Food? createdFood = await repo.CreateFoodFromExistingAsync(
            ingredientWeights,
            "Food mix",
            "");

        if (createdFood is null) {
            return;
        }

        Food.Food? insertedFood = await repo.AddFoodAsync(createdFood, userId, true);
        Console.WriteLine($"Inserted food: {ToReadableString(insertedFood)}");
        Console.WriteLine("=====================");
        Console.WriteLine("Ingredients:");

        foreach (Food.Food f in insertedFood.Ingredients) {
            Console.WriteLine(f.Name);
            Console.WriteLine(f.Weight);
        }
    }
}