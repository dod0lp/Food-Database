using Food;
using Food_Database.Database.Descriptors;
using Food_Database.Models;
using FoodParser;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static Food.Food;
using static Food_Database.Database.Operations.EFLoader;

namespace ProgramTestFood;
public static class ProgramTestFood {
    private static readonly int UserId = Random.Shared.Next(1, userCountWanted+1);
    const int userCountWanted = 30;
    const int foodCountWanted = 250;

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

        Console.WriteLine("User is logged in as user ID: " + UserId);

        while (true) {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("1 - Browse foods");
            Console.WriteLine("2 - Get food by ID");
            Console.WriteLine("3 - Create new food and favorite it");
            Console.WriteLine("4 - Favorite existing food");
            Console.WriteLine("5 - Show my favorites");
            Console.WriteLine("6 - Test favorite food options");
            Console.WriteLine("7 - Create new composite food from existing (non-composite) food");
            Console.WriteLine("8 - Create new composite food from existing composite food");
            Console.WriteLine("9 - Test get food ingredient tree");
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
                await FavoriteExistingFoodAsync(db, UserId);
                break;

                case "5":
                await ShowFavoritesWithOptionsAsync(db, UserId);
                break;

                case "6":
                await TestFavoriteFoodOptions(db, UserId);
                break;

                case "7":
                await TestCreateFoodFromExisting(db, UserId);
                break;

                case "8":
                await TestCreateFoodFromExistingComposite(db, UserId);
                break;

                case "9": {

                    Console.Write("Food ID: ");
                    Console.Write("Food ID: ");

                    if (!int.TryParse(Console.ReadLine(), out int foodId)) {
                        Console.Write("oh no");
                        break;
                    }

                    string res = await TestGetFoodIngredientTreeAsync(db, foodId);
                    Console.WriteLine(res);
                    break;
                }

                case "0":
                return;
            }
        }
    }

    private static async Task EnsureDummyData(DB_FoodContext db) {
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

    private static async Task TestFavoriteFoodOptions(DB_FoodContext db, int userId) {
        var repo = new FoodRepository(db);
        var ct = CancellationToken.None; // default is none so it's w.e. if used here
        var rng = Random.Shared;
        int count = rng.Next(1, 6);
        int foodId = rng.Next(1, 251);

        IEnumerable<(decimal Weight, decimal Price)> options =
            Enumerable.Range(0, count)
                .Select(_ => (
                    Weight: Math.Round((decimal)(rng.NextDouble() * 199 + 1), 2),
                    Price: Math.Round((decimal)(rng.NextDouble() * 249 + 1), 2)
                ));

        foreach (var (Weight, Price) in options) {
            await repo.AddFavoriteFoodOptionAsync(
                userId,
                foodId,
                new(Weight, Price),
                ct);
        }

        await repo.SaveChangesDBAsync();

        await ShowFavoritesWithOptionsAsync(db, userId);
    }

    private static async Task TestCreateFoodFromExisting(DB_FoodContext db, int userId) {
        var repo = new FoodRepository(db);
        Dictionary<int, decimal> ingredientWeights;

        // can throw if id--key is added twice should be max P(4/250) tho
        try {
            ingredientWeights =
                Enumerable
                    .Range(0, Random.Shared.Next(2, 5)) // 2-4ingr
                        .ToDictionary(
                            _ => Random.Shared.Next(1, 251), // ID
                            _ => Math.Round((decimal)(Random.Shared.NextDouble() * 245 + 5), 2) // weight 5-250
            );
        } catch (ArgumentException) {
            Console.WriteLine("Duplicate food ID generated, abort mission.");
            return;
        }

        Food.Food? createdFood = await repo.CreateFoodFromExistingAsync(
            ingredientWeights,
            "Food mix",
            "Test composite food");

        if (createdFood is null) {
            return;
        }

        Food.Food? insertedFood = await
                        repo.AddFoodAsync(createdFood, userId, true);

        if (insertedFood is null) {
            Console.WriteLine("Failed to insert food into database.");
            return;
        }

        Console.WriteLine($"Inserted food: {ToReadableString(insertedFood)}");
        Console.WriteLine("=====================");
        Console.WriteLine("Ingredients:");

        foreach (Food.Food f in insertedFood.Ingredients) {
            Console.WriteLine(f.Name);
            Console.WriteLine(f.Weight);
        }
    }

    private static async Task TestCreateFoodFromExistingComposite(DB_FoodContext db, int userId) {
        var repo = new FoodRepository(db);

        Dictionary<int, decimal> ingredientWeights = Enumerable
            .Range(251, 10)
            .OrderBy(_ => Random.Shared.Next())
            .Take(2)
            .ToDictionary(
                id => id,
                _ => Math.Round(
                    (decimal)(Random.Shared.NextDouble() * 245 + 5),
                    2));

        List<Food.Food> ingredients = new();

        Nutrients nutrients = new(
            new Energy(0),
            new Fat(0, 0),
            new Carbohydrates(0, 0),
            new Protein(0),
            new Salt(0));

        double totalWeight = 0;

        foreach (var item in ingredientWeights) {
            Food.Food? ingredient = await repo.GetFoodAsync(item.Key);

            if (ingredient is null) {
                throw new InvalidOperationException(
                    $"Food {item.Key} does not exist.");
            }

            double weight = (double)item.Value;

            ingredient.Weight = weight;

            ingredient.NutrientContent =
                (weight / DB_Food_Descriptors.NormalizedWeight) *
                ingredient.NutrientContent;

            ingredients.Add(ingredient);

            totalWeight += weight;
            nutrients += ingredient.NutrientContent;
        }

        Food.Food? food = new(
            0,
            "Composite from composites",
            totalWeight,
            nutrients,
            "Test composite from composite food",
            ingredients);

        Food.Food? insertedFood =
                        await repo.AddFoodAsync(food, userId, true);

        if (insertedFood is null) {
            Console.WriteLine("Failed to insert food into database.");
            return;
        }

        Console.WriteLine($"Inserted food: {ToReadableString(insertedFood)}");
        Console.WriteLine("=====================");
        Console.WriteLine("Ingredients:");

        foreach (Food.Food f in insertedFood.Ingredients) {
            Console.WriteLine(f.Name);
            Console.WriteLine(f.Weight);
        }
    }

    public static async Task<string> TestGetFoodIngredientTreeAsync(
    DB_FoodContext db,
    int foodId,
    double weight = DB_Food_Descriptors.NormalizedWeight,
    CancellationToken cancellationToken = default) {
        Food_DBEntity? food = await db.Food
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == foodId,
                cancellationToken);

        if (food is null) {
            return $"Food ID {foodId} not found.";
        }

        StringBuilder result = new();

        await GetFoodIngredientTreeRecursiveAsync(
            db,
            food,
            weight,
            0,
            new HashSet<int>(),
            result,
            cancellationToken);

        return result.ToString();
    }

    private static async Task GetFoodIngredientTreeRecursiveAsync(
    DB_FoodContext db,
    Food_DBEntity food,
    double weight,
    int depth,
    HashSet<int> path,
    StringBuilder result,
    CancellationToken cancellationToken) {
        var repo = new FoodRepository(db);
        string indent = new(' ', depth * 4);
        double factor =
            weight / DB_Food_Descriptors.NormalizedWeight;

        result.AppendLine(
            $"{indent}{food.Id}: {food.Name} - {weight:0.##} g");
        AppendVal(food.Energy_Kcal, result, indent, factor, "Energy");
        AppendVal(food.Fat_Total, result, indent, factor, "Fat (Total)");
        AppendVal(food.Fat_Saturated, result, indent, factor, "Fat (Saturated)");
        AppendVal(food.Carbs_Total, result, indent, factor, "Carbs");
        AppendVal(food.Carbs_Sugar, result, indent, factor, "Carbs");
        AppendVal(food.Protein_Total, result, indent, factor, "Protein");
        AppendVal(food.Salt_Total, result, indent, factor, "Salt");

        if (!path.Add(food.Id)) {
            result.AppendLine(
                $"{indent}  [Circular reference]");
            return;
        }

        List<FoodIngredients_DBEntity> ingredients =
            await repo.GetIngredientsReadOnly(food.Id,
                                            cancellationToken);

        foreach (FoodIngredients_DBEntity relation in ingredients) {
            double ingredientWeight =
                (weight * (double)relation.Weight_Ingredient_Normalised) /
                DB_Food_Descriptors.NormalizedWeight;

            await GetFoodIngredientTreeRecursiveAsync(
                db,
                relation.Ingredient_Food,
                ingredientWeight,
                depth + 1,
                path,
                result,
                cancellationToken);
        }

        path.Remove(food.Id);
    }

    private static void AppendVal(
    decimal? val, StringBuilder result, string indent,
    double factor, string what, string unit = "g") {
        result.AppendLine(
                    $"{indent}  {what}: {GetValue(val, factor):0.##} {unit}");
    }

    private static double GetValue(decimal? value, double factor) {
        return value.HasValue
            ? (double)value.Value * factor
            : -1;
    }
}