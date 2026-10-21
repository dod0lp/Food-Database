using Food_Database.Database.Descriptors;
using Food_Database.Models;
using FoodBase;
using FoodParser;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Foods = Food_Database.Database.Repositories.Foods;

namespace FoodDatabase.Maintenance;

/// <summary>
/// Runs database seed and constraint-check maintenance commands.
/// </summary>
internal static class Program {
    private const int UserCountWanted = 30;
    private const int FoodCountWanted = 250;
    private const int SimpleMixCount = 30;
    private const int CompositeMixCount = 75;

    private static readonly string ReportsDirectory =
        Environment.GetEnvironmentVariable("FOOD_DATABASE_REPORTS_DIRECTORY") ??
        Path.Combine(
            Food_Database.ProjectPaths.ProjectRoot,
            "backend-maintenance",
            "reports");

    private static readonly string CONSTRAINT_REPORT = Path.Combine(
        ReportsDirectory,
        "database-constraints-check.txt");

    private static readonly FoodNutrient[] MassNutrients = [
        FoodNutrient.Fat_Total,
        FoodNutrient.Carbs_Total,
        FoodNutrient.Protein_Total,
        FoodNutrient.Salt_Total
    ];

    private static readonly NutrientSubsetDefinition[] NutrientSubsetDefinitions = [
        new(FoodNutrient.Fat_Total, FoodNutrient.Fat_Saturated),
        new(FoodNutrient.Carbs_Total, FoodNutrient.Carbs_Sugar)
    ];

    /// <summary>
    /// Runs the requested maintenance command. Seeding completes before checking when both are supplied.
    /// </summary>
    private static async Task<int> Main(string[] args) {
        bool seed = args.Contains("--seed", StringComparer.OrdinalIgnoreCase);
        bool checkDatabase = args.Contains("--checkdb", StringComparer.OrdinalIgnoreCase);
        if ((!seed && !checkDatabase) || args.Length != (seed ? 1 : 0) + (checkDatabase ? 1 : 0)) {
            Console.Error.WriteLine("Use --seed, --checkdb, or both.");
            return 1;
        }

        try {
            Directory.CreateDirectory(ReportsDirectory);
            DbContextOptions<DB_FoodContext> options =
                new DbContextOptionsBuilder<DB_FoodContext>()
                    .UseSqlServer(
                        DB_Food_Descriptors.ConnectionString,
                        sqlOptions => sqlOptions.EnableRetryOnFailure())
                    .Options;
            await using DB_FoodContext db = new(options);

            if (seed) {
                await EnsureDummyData(db);
                Console.WriteLine("Seed data is ready.");
            }

            if (checkDatabase) {
                await CheckDBConstraintsAsync(db);
                Console.WriteLine($"Data checked. Results: {CONSTRAINT_REPORT}");
            }

            return 0;
        } catch (Exception exception) when (IsDatabaseException(exception)) {
            Console.Error.WriteLine("Database operation failed.");
            Console.Error.WriteLine($"Details: {exception.GetBaseException().Message}");
            return 1;
        }
    }

    /// <summary>
    /// Determines if exception was caused by a database operation.
    /// </summary>
    private static bool IsDatabaseException(Exception exception) =>
        exception is SqlException or DbUpdateException or TimeoutException ||
        exception.InnerException is not null &&
            IsDatabaseException(exception.InnerException);

    /// <summary>
    /// Appends database IDs to generated composite-food names.
    /// </summary>
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

    /// <summary>
    /// Function to seed database with dummy data. Using csv file by <see cref="CsvParser"/>.
    /// </summary>
    private static async Task EnsureDummyData(DB_FoodContext db) {
        // add users
        if (await db.Users.Take(UserCountWanted).CountAsync() < UserCountWanted) {
            List<Users_DBEntity> users = [..
                Enumerable.Range(0, UserCountWanted)
                    .Select(index => new Users_DBEntity {
                        UserName = $"seed-user-{index + 1}",
                        NormalizedUserName = $"SEED-USER-{index + 1}"
                    })
            ];

            db.Users.AddRange(users);
        }

        int wanted = FoodCountWanted;
        // fill dummy simple foods
        if (await db.Food.Take(wanted).CountAsync() < wanted) {
            await FoodCsvMap.ParseCsvIntoDB(db, FoodCountWanted);

            await db.SaveChangesAsync();
        }

        wanted += SimpleMixCount;
        // create mixed foods from simple foods without ingredients
        if (await db.Food.Take(wanted).CountAsync() < wanted) {
            for (int i = 0; i < SimpleMixCount; i++) {
                await CreateSeedCompositeFromSimpleFoodsAsync(
                    db,
                    Random.Shared.Next(1, UserCountWanted + 1));
            }

            await db.SaveChangesAsync();
        }

        wanted += CompositeMixCount;
        // create mixed foods from mixed foods
        if (await db.Food.Take(wanted).CountAsync() < wanted) {
            for (int i = 0; i < CompositeMixCount; i++) {
                await CreateSeedCompositeFromCompositeFoodsAsync(
                    db,
                    Random.Shared.Next(1, UserCountWanted + 1));
            }

            await db.SaveChangesAsync();
        }

        await HelperAppendFoodID(db, FoodCountWanted + 1);

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Creates one generated composite food from simple seed foods.
    /// </summary>
    /// <param name="db"></param>
    /// <param name="userId"></param>
    /// <param name="silenced"></param>
    /// <returns></returns>
    private static async Task CreateSeedCompositeFromSimpleFoodsAsync(
    DB_FoodContext db,
    int userId,
    bool silenced = true) {
        var repo = new Foods.Repository(db);
        Dictionary<int, decimal> ingredientWeights;

        var toRestore = Console.Out;
        if (silenced) {
            Console.SetOut(TextWriter.Null);
        }

        ingredientWeights = Enumerable
            .Range(1, FoodCountWanted)
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

    /// <summary>
    /// Creates one generated composite food from existing composite seed foods.
    /// </summary>
    private static async Task CreateSeedCompositeFromCompositeFoodsAsync(
    DB_FoodContext db,
    int userId,
    bool silenced = true) {
        var repo = new Foods.Repository(db);

        var toRestore = Console.Out;
        if (silenced) {
            Console.SetOut(TextWriter.Null);
        }

        Dictionary<int, decimal> ingredientWeights = Enumerable
            .Range(251, SimpleMixCount)
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

        await File.WriteAllTextAsync(CONSTRAINT_REPORT, report.ToString(),
                                                    cancellationToken);
    }

    /// <summary>
    /// Helper function to get all violation based on set rules.
    /// </summary>
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

    /// <summary>
    /// Enum representing food nutrients, without calories.
    /// </summary>
    private enum FoodNutrient {
        Fat_Total,
        Fat_Saturated,
        Carbs_Total,
        Carbs_Sugar,
        Protein_Total,
        Salt_Total
    }

    /// <summary>
    /// Abstract record representing some food constraint violation.
    /// </summary>
    private abstract record FoodConstraintViolation(
    int FoodId,
    string FoodName) {
        /// <summary>
        /// Specific violation detail line for the report.
        /// </summary>
        public abstract string ToReportLine();
    }

    /// <summary>
    /// Record representing a violation of a nutrient bound for a food.
    /// </summary>
    private sealed record FoodNutrientBoundViolation(
    int FoodId,
    string FoodName,
    FoodNutrient Nutrient,
    decimal Value) : FoodConstraintViolation(FoodId, FoodName) {
        public override string ToReportLine() =>
            $"  Food {FoodId} ({FoodName}): " +
            $"{Nutrient} = {Value:0.##}";
    }

    /// <summary>
    /// Record representing a violation of the total nutrient mass for a food.
    /// </summary>
    private sealed record FoodNutrientTotalViolation(
    int FoodId,
    string FoodName,
    decimal Total) : FoodConstraintViolation(FoodId, FoodName) {
        public override string ToReportLine() =>
            $"  Food {FoodId} ({FoodName}): {Total:0.##} g";
    }

    /// <summary>
    /// Record representing a violation where a nutrient subset exceeds its total for a food.
    /// </summary>
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

    /// <summary>
    /// Record representing a definition of a nutrient subset and its total.
    /// </summary>
    private sealed record NutrientSubsetDefinition(
                FoodNutrient Total, FoodNutrient Subset);

}