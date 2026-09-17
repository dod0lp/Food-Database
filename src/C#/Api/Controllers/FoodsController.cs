using Food_Database.Api.Contracts;
using Foods = Food_Database.Database.Repositories.Foods;
using FoodBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using static Food_Database.Database.Repositories.Foods.Mapper;
using Food_Database.Database.Repositories.Foods;
using static Food_Database.Database.Descriptors.DB_Food_Descriptors;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Food_Database.Api.Controllers;

[ApiController]
[Route("api/foods")]
public sealed class FoodsController : ControllerBase {
    /// <summary>
    /// The repository for managing food data from database.
    /// </summary>
    private readonly Foods.Repository _foods;

    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 50;

    public FoodsController(Foods.Repository foods) {
        _foods = foods;
    }

    /// <summary>
    /// Helper function to parse a nullable decimal to a double with a default value if null.
    /// </summary>
    /// <param name="value">Value to parse.</param>
    /// <param name="defaultValue">Default value to use if the input is null.</param>
    /// <returns>The parsed double value or the default value.</returns>
    private static double Val(decimal? value, double defaultValue = Mapper.Unknown) {
        return 
            value.HasValue
            ? (double)value.Value
            : defaultValue;
    }

    /// <summary>
    /// Gets a list of system foods.
    /// </summary>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>List of foods with their total.</returns>
    [HttpGet]
    public async Task<ActionResult<FoodPageResponse>> GetSystemFoods(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = DefaultPageSize) {
        if (page < 1 || IsValidPageSize(pageSize)) {
            return _BadPageSize();
        }

        List<Food> items = await _foods.GetSystemFoodsAsync(
            page, pageSize, HttpContext.RequestAborted);

        int total = await
            _foods.GetSystemFoodCountAsync(HttpContext.RequestAborted);

        return Ok(new FoodPageResponse(items, total));
    }

    /// <summary>
    /// Helper wrapper to return a BadRequest for page.
    /// </summary>
    /// <returns><see cref="BadRequestObjectResult"/> with a message about invalid page or pageSize</returns>
    private ActionResult<FoodPageResponse> _BadPageSize() {
        return BadRequest($"Page must be positive and pageSize must be from {MinPageSize} to {MaxPageSize}.");
    }

    /// <summary>
    /// Helper function to see if pagesize is valid.
    /// </summary>
    /// <param name="pageSize">The page size to validate.</param>
    /// <returns><c>true</c> if the page size is valid.<br></br>
    /// <c>false</c> otherwise.</returns>
    private static bool IsValidPageSize(int pageSize) {
        return pageSize is >= MinPageSize and <= MaxPageSize;
    }

    /// <summary>
    /// Gets a list of foods created by the authenticated user.
    /// </summary>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<FoodPageResponse>> GetMyFoods(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = DefaultPageSize) {
        int? userId = CurrentUserId();
        if (userId is null) {
            return Unauthorized();
        }
        if (page < 1 || IsValidPageSize(pageSize)) {
            return _BadPageSize();
        }

        List<Food> items = await
            _foods.GetUserCreatedFoodsAsync(
                userId.Value, page, pageSize, HttpContext.RequestAborted);

        int total = await
            _foods.GetUserCreatedFoodCountAsync(
                userId.Value, HttpContext.RequestAborted);

        return Ok(new FoodPageResponse(items, total));
    }

    /// <summary>
    /// Gets a single food by its ID.
    /// </summary>
    /// <param name="foodId">ID of the food.</param>
    /// <returns>Created <see cref="Task"/> of <see cref="ActionResult{Food}"/></returns>
    [HttpGet("{foodId:int}")]
    public async Task<ActionResult<Food>> GetOne(int foodId) {
        Food? food = await _foods.GetFoodAsync(foodId, HttpContext.RequestAborted);
        return food is null ? NotFound() : Ok(food);
    }

    /// <summary>
    /// Creates a simple food with the provided details.<br></br>
    /// The authenticated user will be set as the creator of the food, and it will be marked as a favorite for that user.
    /// </summary>
    /// <param name="request">Details of the food to create.</param>
    /// <returns>Created <see cref="Task"/> of <see cref="ActionResult{Food}"/></returns></returns>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Food>> CreateSimpleFood(CreateSimpleFoodRequest request) {
        int? userId = CurrentUserId();
        if (userId is null || string.IsNullOrWhiteSpace(request.Name)) {
            return BadRequest("A name and authenticated user are required.");
        }

        var nutrients = new Nutrients(
            new Energy(Val(request.EnergyKcal)),
            new Fat(Val(request.FatTotal), Val(request.FatSaturated)),
            new Carbohydrates(Val(request.CarbsTotal), Val(request.CarbsSugar)),
            new Protein(Val(request.ProteinTotal)),
            new Salt(Val(request.SaltTotal)));
        nutrients.RoundUp2decimal();

        var food = new Food(
            -1,
            request.Name.Trim(),
            NormalizedWeight,
            nutrients,
            request.Description ?? string.Empty);

        Food created = await
            _foods.AddFoodAsync(food, userId.Value,
                        cancellationToken: HttpContext.RequestAborted);
        await _foods.SetFavoriteFoodAsync(userId.Value, created.Id,
                        cancellationToken: HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetOne),
                            new { foodId = created.Id }, created);
    }

    /// <summary>
    /// Creates a composite food from existing ingredients.
    /// </summary>
    /// <param name="request">Details of the composite food to create.</param>
    /// <returns>Created <see cref="Task"/> of <see cref="ActionResult{Food}"/></returns>
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
            .ToDictionary(group => group.Key,
                            group => group.Sum(x => x.WeightInGrams));

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
            : CreatedAtAction(nameof(GetOne),
                    new { foodId = created.Id }, created);
    }

    /// <summary>
    /// Checks if a specific food is marked as a favorite by the authenticated user.
    /// </summary>
    /// <param name="foodId">ID of a food.</param>
    /// <returns><b>true</b> if the food is a favorite.<br></br>
    /// <b>false</b> otherwise.</returns>
    [Authorize]
    [HttpGet("{foodId:int}/favorite")]
    public async Task<ActionResult<bool>> IsFavorite(int foodId) {
        int? userId = CurrentUserId();
        if (userId is null) {
            return Unauthorized();
        }

        return Ok(await
                _foods.IsUserFavoriteFoodAsync(
                    userId.Value, foodId,
                    HttpContext.RequestAborted));
    }

    /// <summary>
    /// Gets a list of all favorite foods for the authenticated user, with theirs options.
    /// </summary>
    /// <returns>List of user's <see cref="Food.FavoriteFood"/>.</returns>
    [Authorize]
    [HttpGet("favorites")]
    public async Task<ActionResult<List<Food.FavoriteFood>>> GetFavorites() {
        int? userId = CurrentUserId();

        return userId is null
            ? Unauthorized()
            : Ok(await 
                    _foods.GetFavoritesWithOptionsAsync(userId.Value,
                                            HttpContext.RequestAborted));
    }

    /// <summary>
    /// Sets a specific food as a favorite for the authenticated user, with optional remark and options.
    /// </summary>
    /// <param name="foodId">ID of the food.</param>
    /// <param name="request">Request containing the favorite food anbd its details.</param>
    /// <returns>The <see cref="Task"/> of updated favorite food.</returns>
    [Authorize]
    [HttpPut("{foodId:int}/favorite")]
    public async Task<ActionResult<Food>> SetFavorite(
    int foodId,
    SetFavoriteFoodRequest request) {
        int? userId = CurrentUserId();
        if (userId is null) {
            return Unauthorized();
        }

        IEnumerable<Food.FavoriteFoodOption>? options = 
            request.Options?.Select(option =>
                new Food.FavoriteFoodOption(option.Weight, option.Price));

        Food? food = await _foods.SetFavoriteFoodAsync(
            userId.Value,
            foodId,
            request.Remark,
            options,
            HttpContext.RequestAborted);

        return food is not null
                ? Ok(food)
                : NotFound();
    }

    /// <summary>
    /// Removes a specific food from the authenticated user's favorites.<br></br>
    /// Optionally, it can also delete user's food favorite data for this food.
    /// </summary>
    /// <param name="foodId">ID of the food to remove from favorites.</param>
    /// <param name="deletePersonalData">If to also delete user's food favorite data for thisfood.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Removes a specific weight option of a favorite food for the authenticated user.
    /// </summary>
    /// <param name="foodId">ID of the food to remove its option.</param>
    /// <param name="weight">Weight (primary key) of the option to remove.</param>
    /// <returns><see cref="NoContentResult"/> if successfully removed.<br></br>
    /// <see cref="NotFoundResult"/> if the option is not found.</returns>
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
    
    /// <summary>
    /// Helper function to get current user ID.
    /// </summary>
    /// <returns>User ID or null if not found.</returns>
    private int? CurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)
            ? userId
            : null;
}
