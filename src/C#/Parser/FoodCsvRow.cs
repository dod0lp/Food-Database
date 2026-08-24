using CsvHelper;
using CsvHelper.Configuration;
using Food_Database.Models;
using System.Globalization;

namespace FoodParser;
public class FoodCsv {
    public string? Name { get; set; }
    public decimal? Calories { get; set; }
    public decimal? Fat { get; set; }
    public decimal? SaturatedFat { get; set; }
    public decimal? Carbs { get; set; }
    public decimal? Sugar { get; set; }
    public decimal? Protein { get; set; }
}

public sealed class FoodCsvMap : ClassMap<FoodCsv> {
    public FoodCsvMap() {
        Map(x => x.Name).Name("Description");
        Map(x => x.Calories).Name("Data.Kilocalories");
        Map(x => x.Fat).Name("Data.Fat.Total Lipid");
        Map(x => x.SaturatedFat).Name("Data.Fat.Saturated Fat");
        Map(x => x.Carbs).Name("Data.Carbohydrate");
        Map(x => x.Sugar).Name("Data.Sugar Total");
        Map(x => x.Protein).Name("Data.Protein");
    }

    public static async Task ParseCsvIntoDB(DB_FoodContext db) {
        using var reader = new StreamReader("../../../Parser/food.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        const int count = 250;

        csv.Context.RegisterClassMap<FoodCsvMap>();

        var foods = csv.GetRecords<FoodCsv>()
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
    }
}