using Food_Database.Database.Descriptors;
using Food_Database.Models;
using FoodBase;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Foods = Food_Database.Database.Repositories.Foods;

namespace ProgramTestFood;

/// <summary>
/// Formatting and ingredient-tree helpers used by the interactive menu.
/// </summary>
internal static class InteractiveMenuHelpers {
    /// <summary>
    /// Formats an optional nutrient value for console output.
    /// </summary>
    internal static string Format(int? value, string unit) {
        return value.HasValue
            ? $"{value.Value} {unit}"
            : "-1";
    }

    /// <summary>
    /// Formats an optional nutrient value for console output.
    /// </summary>
    internal static string Format(decimal? value, string unit) {
        return value.HasValue
            ? $"{value.Value} {unit}"
            : "-1";
    }

    /// <summary>
    /// Creates a formatted ingredient tree with nutrient values scaled to a weight.
    /// </summary>
    internal static async Task<string> GetFoodIngredientTreeAsync(
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

    /// <summary>
    /// Recursively appends a food and its ingredients to the formatted tree.
    /// </summary>
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