using DotNetEnv;
using Food_Database.Database.Descriptors;
using Food_Database.Database.Repositories.Foods;
using Food_Database.Models;
using FoodBase;
using FoodParser;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Foods = Food_Database.Database.Repositories.Foods;

namespace ProgramTestFood;

public static class ProgramTestFood {
    private static readonly int UserId = Random.Shared.Next(1, userCountWanted + 1);
    const int userCountWanted = 30;
    const int foodCountWanted = 250;
    const int simpleMixCount = 30;
    const int compositeMixCount = 75;

    private static readonly FoodNutrient[] MassNutrients = [
        FoodNutrient.Fat_Total,
        FoodNutrient.Carbs_Total,
        FoodNutrient.Protein_Total,
        FoodNutrient.Salt_Total
    ];

    private static readonly NutrientSubsetDefinition[]
    NutrientSubsetDefinitions = [
        new(FoodNutrient.Fat_Total, FoodNutrient.Fat_Saturated),
        new(FoodNutrient.Carbs_Total, FoodNutrient.Carbs_Sugar)
    ];

    static async Task Main() {
        var options = new DbContextOptionsBuilder<DB_FoodContext>()
            .UseSqlServer(
                DB_Food_Descriptors.ConnectionString,
                sqlOptions => sqlOptions.EnableRetryOnFailure())
            .Options;

        using var db = new DB_FoodContext(options);

        await EnsureDummyData(db);
        await CheckDBConstraintsAsync(db);

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

                case "9":
                string res = await GetFoodIngredientTreeInputAsync(db) ?? "empty";
                Console.WriteLine(res);
                break;

                case "0":
                return;
            }
        }
    }

    private static async Task<int> HelperAppendFoodID(DB_FoodContext db, int startId) {
        CancellationToken ct = default;

        var foods = await db.Food
        .Where(x => x.Id >= startId)
        .Select(x => new {
            x.Id,
            x.Name
        })
        .ToListAsync(ct);

        var idsToUpdate = foods
            .Where(x => !x.Name.EndsWith($" {x.Id}"))
            .Select(x => x.Id)
            .ToList();

        if (idsToUpdate.Count == 0) {
            return 0;
        }

        return
            await db.Food
                .Where(x => idsToUpdate.Contains(x.Id))
                .ExecuteUpdateAsync(
                    x => x.SetProperty(
                        f => f.Name,
                        f => f.Name + " " + f.Id),
                    ct);
    }

    private static async Task EnsureDummyData(DB_FoodContext db) {
        // add users
        if (await db.Users.Take(userCountWanted).CountAsync() < userCountWanted) {
            List<Users_DBEntity> users = [..
                Enumerable.Range(0, userCountWanted)
                    .Select(_ => new Users_DBEntity())
            ];

            db.Users.AddRange(users);
        }

        int wanted = foodCountWanted;
        // fill dummy simple foods
        if (await db.Food.Take(wanted).CountAsync() < wanted) {
            await FoodCsvMap.ParseCsvIntoDB(db, foodCountWanted);

            await db.SaveChangesAsync();
        }

        wanted += simpleMixCount;
        // create mixed foods from simple foods without ingredients
        if (await db.Food.Take(wanted).CountAsync() < wanted) {
            for (int i = 0; i < simpleMixCount; i++) {
                await TestCreateFoodSimple(db, Random.Shared.Next(1, userCountWanted + 1));
            }

            await db.SaveChangesAsync();
        }

        wanted += compositeMixCount;
        // create mixed foods from mixed foods
        if (await db.Food.Take(wanted).CountAsync() < wanted) {
            for (int i = 0; i < compositeMixCount; i++) {
                await TestCreateFoodFromExistingComposite(db, Random.Shared.Next(1, userCountWanted + 1));
            }

            await db.SaveChangesAsync();
        }

        await HelperAppendFoodID(db, foodCountWanted + 1);

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
        var repo = new Foods.Repository(db);
        var ct = CancellationToken.None; // default is none so it's w.e. if used here
        var rng = Random.Shared;
        int count = rng.Next(1, 6);
        int foodId = rng.Next(1, foodCountWanted + 1);

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

    private static async Task TestCreateFoodSimple(DB_FoodContext db, int userId, bool silenced = true) {
        var repo = new Foods.Repository(db);
        Dictionary<int, decimal> ingredientWeights;

        var toRestore = Console.Out;
        if (silenced) {
            Console.SetOut(TextWriter.Null);
        }

        ingredientWeights = Enumerable
            .Range(1, foodCountWanted)
            .OrderBy(_ => Random.Shared.Next())
            .Take(Random.Shared.Next(2, 5))
            .ToDictionary(
                id => id,
                _ => Math.Round(
                    (decimal)(Random.Shared.NextDouble() * 245 + 5), 2)
            );

        Food? insertedFood = await repo.CreateCompositeFoodAsync(
            ingredientWeights,
            "Food mix",
            userId,
            "Test composite food");

        if (insertedFood is null) {
            Console.WriteLine("Failed to insert food into database.");
            return;
        }

        Console.WriteLine($"Inserted food: {Food.ToReadableString(insertedFood)}");
        Console.WriteLine("=====================");
        Console.WriteLine("Ingredients:");

        foreach (Food f in insertedFood.Ingredients) {
            Console.WriteLine(f.Name);
            Console.WriteLine(f.Weight);
        }

        if (silenced) {
            Console.SetOut(toRestore);
        }
    }

    private static async Task TestCreateFoodFromExistingComposite(DB_FoodContext db, int userId, bool silenced = true) {
        var repo = new Foods.Repository(db);

        var toRestore = Console.Out;
        if (silenced) {
            Console.SetOut(TextWriter.Null);
        }

        Dictionary<int, decimal> ingredientWeights = Enumerable
            .Range(251, simpleMixCount)
            .OrderBy(_ => Random.Shared.Next())
            .Take(Random.Shared.Next(2, 4))
            .ToDictionary(
                id => id,
                _ => Math.Round(
                    (decimal)(Random.Shared.NextDouble() * 245 + 5), 2));

        Food? insertedFood = await repo.CreateCompositeFoodAsync(
            ingredientWeights,
            "Composite from composites",
            userId,
            "Test composite from composite food");

        if (insertedFood is null) {
            Console.WriteLine("Failed to insert food into database.");
            return;
        }

        Console.WriteLine($"Inserted food: {insertedFood}");
        Console.WriteLine("=====================");
        Console.WriteLine("Ingredients:");

        foreach (Food f in insertedFood.Ingredients) {
            Console.WriteLine(f.Name);
            Console.WriteLine(f.Weight);
        }

        if (silenced) {
            Console.SetOut(toRestore);
        }
    }

    /// <summary>
    /// Checks the food nutrient bounds and the normalized ingredient-weight
    ///     totals stored in the database.
    /// </summary>
    /// <remarks>Read-only function.</remarks>
    private static async Task CheckDBConstraintsAsync(
    DB_FoodContext db,
    CancellationToken cancellationToken = default) {
        List<FoodConstraintViolation> violations =
            await GetViolations(db, cancellationToken);

        List<FoodNutrientBoundViolation> nutrientViolations = [..
            violations.OfType<FoodNutrientBoundViolation>()];
        List<FoodNutrientTotalViolation> nutrientTotalViolations = [..
            violations.OfType<FoodNutrientTotalViolation>()];
        List<FoodNutrientSubsetViolation> nutrientSubsetViolations = [..
            violations.OfType<FoodNutrientSubsetViolation>()];

        Dictionary<int, decimal> ingredientWeightSumsNotEqualTo100 =
            await GetIngredientWeightSumsNotEqualTo100Async(
                db,
                cancellationToken);

        StringBuilder report = new();
        report.AppendLine("Database constraint check:");

        if (nutrientViolations.Count == 0) {
            report.AppendLine("=== OK === All non-null nutrient gram values are between 0 and 100 per 100g.");
        } else {
            report.AppendLine("=== NOT OK === Nutrient values outside 0-100 per 100g:");

            foreach (FoodNutrientBoundViolation violation in nutrientViolations
                .OrderBy(x => x.FoodId)
                .ThenBy(x => x.Nutrient)) {
                report.AppendLine(violation.ToReportLine());
            }
        }

        if (nutrientTotalViolations.Count == 0) {
            report.AppendLine("=== OK === The known nutrient total is at most 100g for every food.");
        } else {
            report.AppendLine("=== NOT OK === Foods with nutrient total more than 100g:");

            foreach (FoodNutrientTotalViolation violation in nutrientTotalViolations
                .OrderBy(x => x.FoodId)) {
                report.AppendLine(violation.ToReportLine());
            }
        }

        if (nutrientSubsetViolations.Count == 0) {
            report.AppendLine("=== OK === Saturated fat and sugar is less or equal to total fat and carbs.");
        } else {
            report.AppendLine("=== NOT OK === Nutrient subsets larger than their total:");

            foreach (FoodNutrientSubsetViolation violation in
                nutrientSubsetViolations
                .OrderBy(x => x.FoodId)
                .ThenBy(x => x.Subset)) {
                report.AppendLine(violation.ToReportLine());
            }
        }

        if (ingredientWeightSumsNotEqualTo100.Count == 0) {
            report.AppendLine("=== OK === Every composite food's ingredient weights total exactly 100g.");
        } else {
            report.AppendLine("=== NOT OK === Composite foods whose ingredient weights do not total 100g:");

            foreach ((int foodId, decimal totalWeight) in
                ingredientWeightSumsNotEqualTo100.OrderBy(x => x.Key)) {
                decimal difference =
                    totalWeight - (decimal)DB_Food_Descriptors.NormalizedWeight;

                report.AppendLine(
                    $"  Food {foodId}: {totalWeight:0.##} g " +
                    $"(difference: {difference:+0.##;-0.##;0} g)");
            }
        }

        string reportPath = Path.Combine(Environment.CurrentDirectory, "../../database-constraints.txt");
        await File.WriteAllTextAsync(reportPath, report.ToString(),
                                                    cancellationToken);
    }

    private static async Task<List<FoodConstraintViolation>>
    GetViolations(
    DB_FoodContext db,
    CancellationToken cancellationToken) {
        List<FoodNutrientBoundViolation> nutrientViolations =
            await GetFoodNutrientBoundViolationsAsync(db, cancellationToken);

        List<FoodNutrientTotalViolation> nutrientTotalViolations =
            await GetFoodNutrientTotalViolationsAsync(db, cancellationToken);

        List<FoodNutrientSubsetViolation> nutrientSubsetViolations =
            await GetFoodNutrientSubsetViolationsAsync(db, cancellationToken);

        List<FoodConstraintViolation> violations = [];
        violations.AddRange(nutrientViolations);
        violations.AddRange(nutrientTotalViolations);
        violations.AddRange(nutrientSubsetViolations);

        return violations;
    }

    /// <summary>
    /// Gets every non-null nutrient gram value that cannot be valid for a
    /// food normalized to 100g.
    /// </summary>
    /// <remarks>
    /// Energy is intentionally excluded.<br></br>
    /// It is simple range 0-100.
    /// </remarks>
    private static async Task<List<FoodNutrientBoundViolation>>
    GetFoodNutrientBoundViolationsAsync(
    DB_FoodContext db,
    CancellationToken cancellationToken = default) {
        List<Food_DBEntity> foods = await db.Food
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        List<FoodNutrientBoundViolation> violations = new();

        foreach (Food_DBEntity food in foods) {
            foreach (FoodNutrient nutrient in Enum.GetValues<FoodNutrient>()) {
                AddNutrientBoundViolation(violations, food, nutrient);
            }
        }

        return violations;
    }

    /// <summary>
    /// Gets foods whose known component mass exceeds their 100g normalized weight.
    /// </summary>
    /// <remarks>
    /// Sugar and saturated fat are excluded because they are subsets.<br></br>
    /// Null values are simply ignored.
    /// </remarks>
    private static async Task<List<FoodNutrientTotalViolation>>
    GetFoodNutrientTotalViolationsAsync(
    DB_FoodContext db,
    CancellationToken cancellationToken = default) {
        List<Food_DBEntity> foods = await db.Food
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return [.. foods
            .Select(food => new FoodNutrientTotalViolation(
                food.Id,
                food.Name,
                MassNutrients.Sum(nutrient =>
                    GetNutrientValue(food, nutrient) ?? 0m)))
            .Where(x => x.Total > 100m)];
    }

    /// <summary>
    /// Gets foods where a known nutrient subset is greater than its known
    /// total.
    /// </summary>
    /// <remarks>Saturated fat vs total fat, or sugar vs total carbs.</remarks>
    private static async Task<List<FoodNutrientSubsetViolation>>
    GetFoodNutrientSubsetViolationsAsync(
    DB_FoodContext db,
    CancellationToken cancellationToken = default) {
        List<Food_DBEntity> foods = await db.Food
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        List<FoodNutrientSubsetViolation> violations = new();

        foreach (Food_DBEntity food in foods) {
            foreach (NutrientSubsetDefinition definition in
                NutrientSubsetDefinitions) {
                AddNutrientSubsetViolation(
                    violations,
                    food,
                    definition.Total,
                    definition.Subset);
            }
        }

        return violations;
    }

    /// <summary>
    /// Returns a map of composite food ID to its ingredient-weight total,
    ///     for every composite that do not sum to exactly 100g.
    /// </summary>
    private static async Task<Dictionary<int, decimal>>
    GetIngredientWeightSumsNotEqualTo100Async(
    DB_FoodContext db,
    CancellationToken cancellationToken = default) {
        decimal normalizedWeight =
            (decimal)DB_Food_Descriptors.NormalizedWeight;

        return await db.FoodIngredients
            .AsNoTracking()
            .GroupBy(x => x.Food_Id)
            .Select(group => new {
                FoodId = group.Key,
                TotalWeight = group.Sum(x => x.Weight_Ingredient_Normalised)
            })
            .Where(x => x.TotalWeight != normalizedWeight)
            .ToDictionaryAsync(
                x => x.FoodId,
                x => x.TotalWeight,
                cancellationToken);
    }

    /// <summary>
    /// Adds a nutrient violation when a known nutrient value is outside the
    ///     0-100g range for a food normalized to 100g.
    /// </summary>
    private static void AddNutrientBoundViolation(
    List<FoodNutrientBoundViolation> violations,
    Food_DBEntity food,
    FoodNutrient nutrient) {
        decimal? value = GetNutrientValue(food, nutrient);

        if (value is not decimal knownValue ||
                (knownValue >= 0m && knownValue <= 100m)) {
            return;
        }

        violations.Add(new FoodNutrientBoundViolation(
            food.Id,
            food.Name,
            nutrient,
            knownValue));
    }

    /// <summary>
    /// Adds a violation when both values are known and the subset is bigger than the total.
    /// </summary>
    private static void AddNutrientSubsetViolation(
    List<FoodNutrientSubsetViolation> violations,
    Food_DBEntity food,
    FoodNutrient total,
    FoodNutrient subset) {
        decimal? totalValue = GetNutrientValue(food, total);
        decimal? subsetValue = GetNutrientValue(food, subset);

        if (totalValue is not decimal knownTotal ||
                subsetValue is not decimal knownSubset ||
                knownSubset <= knownTotal) {
            return;
        }

        violations.Add(new FoodNutrientSubsetViolation(
            food.Id,
            food.Name,
            total,
            knownTotal,
            subset,
            knownSubset));
    }

    /// <summary>
    /// Gets a <see cref="FoodNutrient"/> value from database <see cref="Food_DBEntity"/> entity.
    /// </summary>
    private static decimal? GetNutrientValue(
    Food_DBEntity food,
    FoodNutrient nutrient) {
        return nutrient switch {
            FoodNutrient.Fat_Total => food.Fat_Total,
            FoodNutrient.Fat_Saturated => food.Fat_Saturated,
            FoodNutrient.Carbs_Total => food.Carbs_Total,
            FoodNutrient.Carbs_Sugar => food.Carbs_Sugar,
            FoodNutrient.Protein_Total => food.Protein_Total,
            FoodNutrient.Salt_Total => food.Salt_Total,
            _ => throw new ArgumentOutOfRangeException(nameof(nutrient))
        };
    }

    private enum FoodNutrient {
        Fat_Total,
        Fat_Saturated,
        Carbs_Total,
        Carbs_Sugar,
        Protein_Total,
        Salt_Total
    }

    private abstract record FoodConstraintViolation(
    int FoodId,
    string FoodName) {
        /// <summary>
        /// Specific violation detail line for the report.
        /// </summary>
        public abstract string ToReportLine();
    }

    private sealed record FoodNutrientBoundViolation(
    int FoodId,
    string FoodName,
    FoodNutrient Nutrient,
    decimal Value) : FoodConstraintViolation(FoodId, FoodName) {
        public override string ToReportLine() =>
            $"  Food {FoodId} ({FoodName}): " +
            $"{Nutrient} = {Value:0.##}";
    }

    private sealed record FoodNutrientTotalViolation(
    int FoodId,
    string FoodName,
    decimal Total) : FoodConstraintViolation(FoodId, FoodName) {
        public override string ToReportLine() =>
            $"  Food {FoodId} ({FoodName}): {Total:0.##} g";
    }

    private sealed record FoodNutrientSubsetViolation(
    int FoodId,
    string FoodName,
    FoodNutrient Total,
    decimal TotalValue,
    FoodNutrient Subset,
    decimal SubsetValue) : FoodConstraintViolation(FoodId, FoodName) {
        public override string ToReportLine() =>
            $"  Food {FoodId} ({FoodName}): " +
            $"{Subset} = {SubsetValue:0.##} g, " +
            $"but {Total} = {TotalValue:0.##} g";
    }

    private sealed record NutrientSubsetDefinition(
                FoodNutrient Total, FoodNutrient Subset);

    public static async Task<string?> GetFoodIngredientTreeInputAsync(DB_FoodContext db) {
        Console.Write("Food ID: ");

        if (!int.TryParse(Console.ReadLine(), out int foodId)) {
            Console.Write("oh no");
            return null;
        }

        string res = await GetFoodIngredientTreeAsync(db, foodId);
        return res;
    }

    public static async Task<string> GetFoodIngredientTreeAsync(
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
        var repo = new Foods.Repository(db);
        string indent = new(' ', depth * 4);
        double factor =
            weight / DB_Food_Descriptors.NormalizedWeight;

        result.AppendLine(
            $"{indent}{food.Id}: {food.Name} - {weight:0.##} g");
        AppendVal(food.Energy_Kcal, result, indent, factor, "Energy", "kcal");
        AppendVal(food.Fat_Total, result, indent, factor, "Fat (Total)");
        AppendVal(food.Fat_Saturated, result, indent, factor, "Fat (Saturated)");
        AppendVal(food.Carbs_Total, result, indent, factor, "Carbs");
        AppendVal(food.Carbs_Sugar, result, indent, factor, "Sugar");
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

    /// <summary>
    /// Helper function to append nutrient value to the result string.
    /// </summary>
    /// <param name="val">The nutrient value to append.</param>
    /// <param name="result">The string builder to append the value to.</param>
    /// <param name="indent">The indentation string.</param>
    /// <param name="factor">The conversion factor.</param>
    /// <param name="what">The name of the nutrient.</param>
    /// <param name="unit">The unit of the nutrient.</param>
    private static void AppendVal(
    decimal? val, StringBuilder result, string indent,
    double factor, string what, string unit = "g") {
        result.AppendLine(
                    $"{indent} " +
                    $"{what}: " +
                    $"{GetValue(val, factor):0.##} {unit}");
    }

    /// <summary>
    /// Helper function to get the nutrient value with the conversion factor applied. Or -1 for non-existent.
    /// </summary>
    /// <param name="value">The nutrient value.</param>
    /// <param name="factor">The conversion factor.</param>
    /// <returns>The converted nutrient value.</returns>
    private static double GetValue(decimal? value, double factor) {
        return value.HasValue
            ? (double)(value.Value) * factor
            : -1;
    }
}
