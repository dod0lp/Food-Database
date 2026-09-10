using FoodBase;

namespace Food_Database.Api.Contracts;

public sealed record CompositeIngredientRequest(int FoodId, decimal WeightInGrams);

public sealed record CreateCompositeFoodRequest(
    string Name,
    string? Description,
    IReadOnlyList<CompositeIngredientRequest> Ingredients);

public sealed record FavoriteFoodOptionRequest(decimal Weight, decimal? Price);

public sealed record SetFavoriteFoodRequest(
    string? Remark,
    IReadOnlyList<FavoriteFoodOptionRequest>? Options);
