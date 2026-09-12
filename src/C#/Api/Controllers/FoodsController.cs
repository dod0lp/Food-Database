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
    public async Task<ActionResult<FoodPageResponse>> GetSystemFoods(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50) {
        if (page < 1 || pageSize is < 1 or > 100) {
            return BadRequest("Page must be positive and pageSize must be from 1 to 100.");
        }

        List<Food> items = await _foods.GetSystemFoodsAsync(
            page, pageSize, HttpContext.RequestAborted);
        int total = await _foods.GetSystemFoodCountAsync(HttpContext.RequestAborted);
        return Ok(new FoodPageResponse(items, total));
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<FoodPageResponse>> GetMyFoods(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50) {
        int? userId = CurrentUserId();
        if (userId is null) {
            return Unauthorized();
        }
        if (page < 1 || pageSize is < 1 or > 100) {
            return BadRequest("Page must be positive and pageSize must be from 1 to 100.");
        }

        List<Food> items = await _foods.GetUserCreatedFoodsAsync(
            userId.Value, page, pageSize, HttpContext.RequestAborted);
        int total = await _foods.GetUserCreatedFoodCountAsync(
            userId.Value, HttpContext.RequestAborted);
        return Ok(new FoodPageResponse(items, total));
    }

    [HttpGet("{foodId:int}")]
    public async Task<ActionResult<Food>> GetOne(int foodId) {
        Food? food = await _foods.GetFoodAsync(foodId, HttpContext.RequestAborted);
        return food is null ? NotFound() : Ok(food);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Food>> CreateSimpleFood(CreateSimpleFoodRequest request) {
        int? userId = CurrentUserId();
        if (userId is null || string.IsNullOrWhiteSpace(request.Name)) {
            return BadRequest("A name and authenticated user are required.");
        }

        var nutrients = new Nutrients(
            new Energy(request.EnergyKcal ?? -1),
            new Fat((double)(request.FatTotal ?? -1), (double)(request.FatSaturated ?? -1)),
            new Carbohydrates((double)(request.CarbsTotal ?? -1), (double)(request.CarbsSugar ?? -1)),
            new Protein((double)(request.ProteinTotal ?? -1)),
            new Salt((double)(request.SaltTotal ?? -1)));
        var food = new Food(
            -1,
            request.Name.Trim(),
            100,
            nutrients,
            request.Description ?? string.Empty);

        Food created = await _foods.AddFoodAsync(food, userId.Value, cancellationToken: HttpContext.RequestAborted);
        await _foods.SetFavoriteFoodAsync(userId.Value, created.Id, cancellationToken: HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetOne), new { foodId = created.Id }, created);
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

        if (created is not null) {
            await _foods.SetFavoriteFoodAsync(
                userId.Value,
                created.Id,
                cancellationToken: HttpContext.RequestAborted);
        }

        return created is null
            ? BadRequest("At least one existing ingredient with a positive weight is required.")
            : CreatedAtAction(nameof(GetOne), new { foodId = created.Id }, created);
    }

    [Authorize]
    [HttpGet("{foodId:int}/favorite")]
    public async Task<ActionResult<bool>> IsFavorite(int foodId) {
        int? userId = CurrentUserId();
        if (userId is null) {
            return Unauthorized();
        }

        return Ok(await _foods.IsUserFavoriteFoodAsync(
            userId.Value, foodId, HttpContext.RequestAborted));
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

    [Authorize]
    [HttpDelete("{foodId:int}/favorite")]
    public async Task<IActionResult> RemoveFavorite(
        int foodId,
        [FromQuery] bool deletePersonalData = false) {
        int? userId = CurrentUserId();
        if (userId is null) {
            return Unauthorized();
        }

        return await _foods.RemoveFavoriteFoodAsync(
            userId.Value,
            foodId,
            deletePersonalData,
            HttpContext.RequestAborted)
            ? NoContent()
            : NotFound();
    }

    [Authorize]
    [HttpDelete("{foodId:int}/favorite/options/{weight:decimal}")]
    public async Task<IActionResult> RemoveFavoriteOption(int foodId, decimal weight) {
        int? userId = CurrentUserId();
        if (userId is null) {
            return Unauthorized();
        }

        return await _foods.RemoveFavoriteFoodOptionAsync(
            userId.Value, foodId, weight, HttpContext.RequestAborted)
            ? NoContent()
            : NotFound();
    }

    private int? CurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)
            ? userId
            : null;
}
