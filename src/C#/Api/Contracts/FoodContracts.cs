using FoodBase;

namespace Food_Database.Api.Contracts;

/// <summary>
/// Make one of the ingredient, using ID and Weight how much there shouldb e used.
/// </summary>
/// <param name="FoodId">ID of the food (ingredient).</param>
/// <param name="WeightInGrams">Weight of the ingredient in grams.</param>
public sealed record CompositeIngredientRequest(int FoodId, decimal WeightInGrams);

/// <summary>
/// Request to create a new composite food from ingredients.
/// </summary>
/// <param name="Name">Name of the composite food.</param>
/// <param name="Description">Description of the composite food.</param>
/// <param name="Ingredients">List of ingredients for the composite food.</param>
public sealed record CreateCompositeFoodRequest(
    string Name,
    string? Description,
    IReadOnlyList<CompositeIngredientRequest> Ingredients);

/// <summary>
/// Request to create a new simple food from basic nutritional information.
/// </summary>
/// <param name="Name">Name of the food.</param>
/// <param name="Description">Description of the food.</param>
/// <remarks>Function arguments don't have description, name is enough.</remarks>
public sealed record CreateSimpleFoodRequest(
    string Name,
    string? Description,
    int? EnergyKcal,
    decimal? FatTotal,
    decimal? FatSaturated,
    decimal? CarbsTotal,
    decimal? CarbsSugar,
    decimal? ProteinTotal,
    decimal? SaltTotal);

/// <summary>
/// Request to create a new favorite food option.
/// </summary>
/// <param name="Weight">Weight of the favorite food option (primary key).</param>
/// <param name="Price">Price of the favorite food option.</param>
public sealed record FavoriteFoodOptionRequest(decimal Weight, decimal? Price);

/// <summary>
/// Response containing a paginated list of food items and the total count.
/// </summary>
/// <param name="Items">The list of <see cref="Food"/> items.</param>
/// <param name="Total">The total count of food items.</param>
public sealed record FoodPageResponse(IReadOnlyList<Food> Items, int Total);

/// <summary>
/// Request to set favorite food remark and options.
/// </summary>
/// <param name="Remark">A remark about the favorite food options.</param>
/// <param name="Options">The list of favorite food options.</param>
public sealed record SetFavoriteFoodRequest(
    string? Remark,
    IReadOnlyList<FavoriteFoodOptionRequest>? Options);
