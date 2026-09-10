using Food_Database.Api.Contracts;
using Food_Database.Database.Repositories.Foods;
using FoodBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Food_Database.Api.Controllers;

[ApiController]
[Route("api/foods")]
public sealed class FoodsController : ControllerBase {
    private readonly Repository _foods;

    public FoodsController(Repository foods) {
        _foods = foods;
    }

    [HttpGet]
    public async Task<ActionResult<List<Food>>> GetMany(
        [FromQuery] int from = 1,
        [FromQuery] int to = 20) {
        if (to - from > 99) {
            return BadRequest("Request at most 100 foods at a time.");
        }

        return Ok(await _foods.GetFoodsAsync(from, to, HttpContext.RequestAborted));
    }

    [HttpGet("{foodId:int}")]
    public async Task<ActionResult<Food>> GetOne(int foodId) {
        Food? food = await _foods.GetFoodAsync(foodId, HttpContext.RequestAborted);
        return food is null ? NotFound() : Ok(food);
    }

    [Authorize]
    [HttpPost("composites")]
    public async Task<ActionResult<Food>> CreateComposite(CreateCompositeFoodRequest request) {
        int? userId = CurrentUserId();
        if (userId is null || string.IsNullOrWhiteSpace(request.Name) ||
            request.Ingredients is null) {
            return BadRequest("A name and authenticated user are required.");
        }

        Dictionary<int, decimal> ingredients = request.Ingredients
            .GroupBy(x => x.FoodId)
            .ToDictionary(group => group.Key, group => group.Sum(x => x.WeightInGrams));

        Food? created = await _foods.CreateCompositeFoodAsync(
            ingredients,
            request.Name.Trim(),
            userId.Value,
            request.Description ?? string.Empty,
            HttpContext.RequestAborted);

        return created is null
            ? BadRequest("At least one existing ingredient with a positive weight is required.")
            : CreatedAtAction(nameof(GetOne), new { foodId = created.Id }, created);
    }

    [Authorize]
    [HttpGet("favorites")]
    public async Task<ActionResult<List<Food.FavoriteFood>>> GetFavorites() {
        int? userId = CurrentUserId();
        return userId is null
            ? Unauthorized()
            : Ok(await _foods.GetFavoritesWithOptionsAsync(userId.Value, HttpContext.RequestAborted));
    }

    [Authorize]
    [HttpPut("{foodId:int}/favorite")]
    public async Task<ActionResult<Food>> SetFavorite(
        int foodId,
        SetFavoriteFoodRequest request) {
        int? userId = CurrentUserId();
        if (userId is null) {
            return Unauthorized();
        }

        IEnumerable<Food.FavoriteFoodOption>? options = request.Options?.Select(option =>
            new Food.FavoriteFoodOption(option.Weight, option.Price));
        Food? food = await _foods.SetFavoriteFoodAsync(
            userId.Value,
            foodId,
            request.Remark,
            options,
            HttpContext.RequestAborted);

        return food is null ? NotFound() : Ok(food);
    }

    private int? CurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)
            ? userId
            : null;
}
