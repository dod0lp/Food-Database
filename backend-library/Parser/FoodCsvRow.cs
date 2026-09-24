using CsvHelper;
using CsvHelper.Configuration;
using Food_Database.Models;
using System.Globalization;

namespace FoodParser;

/// <summary>
/// Represents a row in the food CSV file
/// </summary>
/// <remarks>Should be normalized to 100g</remarks>
public class FoodCsvRow {
    public string? Name { get; set; }
    public decimal? Calories { get; set; }
    public decimal? Fat { get; set; }
    public decimal? SaturatedFat { get; set; }
    public decimal? Carbs { get; set; }
    public decimal? Sugar { get; set; }
    public decimal? Protein { get; set; }
}

/// <summary>
/// Maps the CSV columns to the FoodCsvRow for parsing.
/// </summary>
public sealed class FoodCsvMap : ClassMap<FoodCsvRow> {
    private static readonly string CsvPath = Path.Combine(
        Food_Database.ProjectPaths.ProjectRoot, "data", "seed_data", "food.csv");

    public FoodCsvMap() {
        Map(x => x.Name).Name("Description");
        Map(x => x.Calories).Name("Data.Kilocalories");
        Map(x => x.Fat).Name("Data.Fat.Total Lipid");
        Map(x => x.SaturatedFat).Name("Data.Fat.Saturated Fat");
        Map(x => x.Carbs).Name("Data.Carbohydrate");
        Map(x => x.Sugar).Name("Data.Sugar Total");
        Map(x => x.Protein).Name("Data.Protein");
    }

    /// <summary>
    /// Parses the food CSV file and inserts parsed data into the database.
    /// </summary>
    /// <param name="db">DB context of database where to insert food.</param>
    /// <param name="count">Number of rows to parse.</param>
    /// <returns>Empty <see cref="Task"/></returns>
    /// <exception cref="FileNotFoundException">When file doesn't exist.</exception>
    public static async Task ParseCsvIntoDB(DB_FoodContext db, int count) {
        if (!File.Exists(CsvPath)) {
            throw new FileNotFoundException("Seed CSV file was not found.", CsvPath);
        }

        using var reader = new StreamReader(CsvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<FoodCsvMap>();

        var foods = csv.GetRecords<FoodCsvRow>()
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new Food_DBEntity {
                Name = x.Name is null ? "No-Name" : x.Name[..Math.Min(x.Name.Length, 25)],
                Food_Description = x.Name is null ? "No-Desc" : x.Name,
                Energy_Kcal = (int?)x.Calories,
                Fat_Total = x.Fat,
                Fat_Saturated = x.SaturatedFat,
                Carbs_Total = x.Carbs,
                Carbs_Sugar = x.Sugar,
                Protein_Total = x.Protein,
                Salt_Total = Math.Round((decimal)Random.Shared.NextDouble() * 3, 2)
            })
            .Take(count)
            .ToList();

        db.Food.AddRange(foods);
        await db.SaveChangesAsync();
    }
}
