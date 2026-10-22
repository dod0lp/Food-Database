using Food_Database.Database.Repositories.Foods;
using Food_Database.Models;
using FoodBase;
using Microsoft.EntityFrameworkCore;
using ProgramTestFood;

namespace FoodDatabase.Tests;

/// <summary>
/// Integration tests for repository operations against a temporary SQL Server database.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class FoodRepositoryTests(SqlServerDatabaseFixture database) {
    /// <summary>
    /// Verifies that the fixture creates an empty database.
    /// </summary>
    [Fact]
    public async Task NewDatabaseHasAnEmptyFoodTable() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Assert.Equal(0, await context.Food.CountAsync());
    }

    /// <summary>
    /// Verifies that persisted foods are returned in ID order.
    /// </summary>
    [Fact]
    public async Task GetFoodsReturnsPersistedFoodsInIdOrder() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        context.Food.AddRange(
            new Food_DBEntity { Name = "Apple", Energy_Kcal = 52 },
            new Food_DBEntity { Name = "Banana", Energy_Kcal = 89 });
        await context.SaveChangesAsync();

        Repository repository = new(context);
        var foods = await repository.GetFoodsAsync(1, 10);

        Assert.Collection(
            foods,
            food => Assert.Equal("Apple", food.Name),
            food => Assert.Equal("Banana", food.Name));
    }

    /// <summary>
    /// Verifies that adding a domain food stores its nutrients normalized to 100 grams.
    /// </summary>
    [Fact]
    public async Task AddFoodStoresDomainFoodAsNormalizedDatabaseEntity() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Food domainFood = new(
            id: -1,
            name: "Roasted chickpeas",
            weight: 250d,
            nutrientContent: new Nutrients(
                new Energy(500d),
                new Fat(25d, 5d),
                new Carbohydrates(75d, 12.5d),
                new Protein(20d),
                new Salt(2.5d)),
            description: "Test food");
        Repository repository = new(context);

        Food saved = await repository.AddFoodAsync(domainFood);
        Food_DBEntity entity = await context.Food
            .AsNoTracking()
            .SingleAsync(food => food.Id == saved.Id);

        Assert.Equal("Roasted chickpeas", entity.Name);
        Assert.Equal("Test food", entity.Food_Description);
        Assert.Equal(200, entity.Energy_Kcal);
        Assert.Equal(10m, entity.Fat_Total);
        Assert.Equal(2m, entity.Fat_Saturated);
        Assert.Equal(30m, entity.Carbs_Total);
        Assert.Equal(5m, entity.Carbs_Sugar);
        Assert.Equal(8m, entity.Protein_Total);
        Assert.Equal(1m, entity.Salt_Total);
    }

    /// <summary>
    /// Verifies that stored food values become their equivalent 100-gram domain food.
    /// </summary>
    [Fact]
    public async Task GetFoodMapsDatabaseEntityToDomainFood() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Food_DBEntity entity = new() {
            Name = "Greek yogurt",
            Food_Description = "Plain",
            Energy_Kcal = 97,
            Fat_Total = 5.2m,
            Fat_Saturated = 3.4m,
            Carbs_Total = 3.6m,
            Carbs_Sugar = 3.6m,
            Protein_Total = 9.1m,
            Salt_Total = 0.1m
        };
        context.Food.Add(entity);
        await context.SaveChangesAsync();

        Repository repository = new(context);
        Food food = Assert.IsType<Food>(await repository.GetFoodAsync(entity.Id));

        Assert.Equal(entity.Id, food.Id);
        Assert.Equal("Greek yogurt", food.Name);
        Assert.Equal("Plain", food.Description);
        Assert.Equal(100d, food.Weight);
        Assert.Equal(97d, food.NutrientContent.Energy.Kcal);
        Assert.Equal(5.2d, food.NutrientContent.FatContent.Total);
        Assert.Equal(3.4d, food.NutrientContent.FatContent.Saturated);
        Assert.Equal(3.6d, food.NutrientContent.CarbohydrateContent.Total);
        Assert.Equal(3.6d, food.NutrientContent.CarbohydrateContent.Sugar);
        Assert.Equal(9.1d, food.NutrientContent.Protein.Total);
        Assert.Equal(0.1d, food.NutrientContent.Salt.Total);
    }

    /// <summary>
    /// Verifies that saving a domain composite recursively saves its composite and leaf ingredients.
    /// </summary>
    [Fact]
    public async Task AddFoodPersistsNestedDomainCompositeIngredients() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Food oats = new(-1, "Oats", 60d, CreateNutrients(228d), "");
        Food milk = new(-1, "Milk", 40d, CreateNutrients(24d), "");
        Food breakfast = new(
            -1,
            "Breakfast",
            100d,
            CreateNutrients(252d),
            "",
            [oats, milk]);
        Food banana = new(-1, "Banana", 50d, CreateNutrients(45d), "");
        Food meal = new(
            -1,
            "Breakfast with banana",
            150d,
            CreateNutrients(297d),
            "",
            [breakfast, banana]);
        Repository repository = new(context);

        Food saved = await repository.AddFoodAsync(meal, addIngredients: true);
        List<FoodIngredients_DBEntity> relations = await context.FoodIngredients
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(5, await context.Food.CountAsync());
        Assert.Contains(
            relations,
            relation => relation.Food_Id == saved.Id
                && relation.Ingredient_Food_Id == breakfast.Id);
        Assert.Contains(
            relations,
            relation => relation.Food_Id == saved.Id
                && relation.Ingredient_Food_Id == banana.Id);
        Assert.Contains(
            relations,
            relation => relation.Food_Id == breakfast.Id
                && relation.Ingredient_Food_Id == oats.Id);
        Assert.Contains(
            relations,
            relation => relation.Food_Id == breakfast.Id
                && relation.Ingredient_Food_Id == milk.Id);
    }

    /// <summary>
    /// Verifies that a stored composite of composites becomes a nested domain ingredient tree.
    /// </summary>
    [Fact]
    public async Task GetFoodMapsNestedDatabaseCompositeToDomainTree() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Food_DBEntity oats = new() { Name = "Oats", Energy_Kcal = 380 };
        Food_DBEntity milk = new() { Name = "Milk", Energy_Kcal = 60 };
        Food_DBEntity breakfast = new() { Name = "Breakfast", Energy_Kcal = 252 };
        Food_DBEntity banana = new() { Name = "Banana", Energy_Kcal = 90 };
        Food_DBEntity meal = new() {
            Name = "Breakfast with banana",
            Energy_Kcal = 297
        };
        context.Food.AddRange(oats, milk, breakfast, banana, meal);
        await context.SaveChangesAsync();
        context.FoodIngredients.AddRange(
            new FoodIngredients_DBEntity {
                Food_Id = breakfast.Id,
                Ingredient_Food_Id = oats.Id,
                Weight_Ingredient_Normalised = 60m
            },
            new FoodIngredients_DBEntity {
                Food_Id = breakfast.Id,
                Ingredient_Food_Id = milk.Id,
                Weight_Ingredient_Normalised = 40m
            },
            new FoodIngredients_DBEntity {
                Food_Id = meal.Id,
                Ingredient_Food_Id = breakfast.Id,
                Weight_Ingredient_Normalised = 66.67m
            },
            new FoodIngredients_DBEntity {
                Food_Id = meal.Id,
                Ingredient_Food_Id = banana.Id,
                Weight_Ingredient_Normalised = 33.33m
            });
        await context.SaveChangesAsync();

        Repository repository = new(context);
        Food domainMeal = Assert.IsType<Food>(
            await repository.GetFoodAsync(meal.Id));
        Food domainBreakfast = Assert.Single(
            domainMeal.Ingredients,
            ingredient => ingredient.Name == "Breakfast");

        Assert.Equal("Breakfast with banana", domainMeal.Name);
        Assert.Contains(
            domainMeal.Ingredients,
            ingredient => ingredient.Name == "Banana");
        Assert.Equal(2, domainBreakfast.Ingredients.Count);
        Assert.Contains(
            domainBreakfast.Ingredients,
            ingredient => ingredient.Name == "Oats");
        Assert.Contains(
            domainBreakfast.Ingredients,
            ingredient => ingredient.Name == "Milk");
    }

    /// <summary>
    /// Verifies that a composite food saves its ingredient relations.
    /// </summary>
    [Fact]
    public async Task CreateCompositeFoodPersistsItsIngredients() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Users_DBEntity user = new() { UserName = "composite-user" };
        Food_DBEntity oats = new() { Name = "Oats", Protein_Total = 13m };
        Food_DBEntity milk = new() { Name = "Milk", Protein_Total = 3m };
        context.AddRange(user, oats, milk);
        await context.SaveChangesAsync();

        Repository repository = new(context);
        Food? composite = await repository.CreateCompositeFoodAsync(
            new Dictionary<int, decimal> {
                [oats.Id] = 60m,
                [milk.Id] = 40m
            },
            "Breakfast",
            user.Id,
            "Automated test composite");

        Food saved = Assert.IsType<Food>(composite);
        Assert.Equal("Breakfast", saved.Name);
        Assert.Equal(100d, saved.Weight);
        Assert.Equal(2, saved.Ingredients.Count);
        Assert.True(await repository.IsUserCreatedFoodAsync(saved.Id));
    }

    /// <summary>
    /// Verifies that a composite food can use another composite as an ingredient.
    /// </summary>
    [Fact]
    public async Task CreateCompositeFoodPersistsCompositeIngredients() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Users_DBEntity user = new() { UserName = "nested-composite-user" };
        Food_DBEntity oats = new() { Name = "Oats", Protein_Total = 13m };
        Food_DBEntity milk = new() { Name = "Milk", Protein_Total = 3m };
        context.AddRange(user, oats, milk);
        await context.SaveChangesAsync();

        Repository repository = new(context);
        Food breakfast = Assert.IsType<Food>(
            await repository.CreateCompositeFoodAsync(
                new Dictionary<int, decimal> {
                    [oats.Id] = 60m,
                    [milk.Id] = 40m
                },
                "Breakfast",
                user.Id));

        Food secondBreakfast = Assert.IsType<Food>(
            await repository.CreateCompositeFoodAsync(
                new Dictionary<int, decimal> {
                    [breakfast.Id] = 70m,
                    [milk.Id] = 30m
                },
                "Second breakfast",
                user.Id));

        Assert.Equal("Second breakfast", secondBreakfast.Name);
        Assert.Contains(
            secondBreakfast.Ingredients,
            ingredient => ingredient.Id == breakfast.Id);
    }

    /// <summary>
    /// Verifies that favorites retain their remark and package option.
    /// </summary>
    [Fact]
    public async Task FavoriteFoodStoresItsRemarkAndOptions() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Users_DBEntity user = new() { UserName = "favorite-user" };
        Food_DBEntity food = new() { Name = "Yogurt" };
        context.AddRange(user, food);
        await context.SaveChangesAsync();

        Repository repository = new(context);
        await repository.SetFavoriteFoodAsync(
            user.Id,
            food.Id,
            "Breakfast",
            [new Food.FavoriteFoodOption(150m, 2.5m)]);

        Food.FavoriteFood favorite = Assert.Single(
            await repository.GetFavoritesWithOptionsAsync(user.Id));
        Assert.Equal("Yogurt", favorite.Food.Name);
        Assert.Equal("Breakfast", favorite.Remark);

        Food.FavoriteFoodOption option = Assert.Single(favorite.Options);
        Assert.Equal(150m, option.Weight);
        Assert.Equal(2.5m, option.Price);
    }

    /// <summary>
    /// Verifies that incremental options create a favorite and retain every option.
    /// </summary>
    [Fact]
    public async Task AddFavoriteFoodOptionPersistsMultipleOptions() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Users_DBEntity user = new() { UserName = "option-user" };
        Food_DBEntity food = new() { Name = "Bread" };
        context.AddRange(user, food);
        await context.SaveChangesAsync();

        Repository repository = new(context);
        await repository.AddFavoriteFoodOptionAsync(
            user.Id,
            food.Id,
            new Food.FavoriteFoodOption(100m, 1.2m));
        await repository.AddFavoriteFoodOptionAsync(
            user.Id,
            food.Id,
            new Food.FavoriteFoodOption(250m, 2.7m));
        await repository.SaveChangesDBAsync();

        Food.FavoriteFood favorite = Assert.Single(
            await repository.GetFavoritesWithOptionsAsync(user.Id));
        Assert.Collection(
            favorite.Options,
            option => {
                Assert.Equal(100m, option.Weight);
                Assert.Equal(1.2m, option.Price);
            },
            option => {
                Assert.Equal(250m, option.Weight);
                Assert.Equal(2.7m, option.Price);
            });
    }

    /// <summary>
    /// Verifies the ingredient-tree prompt without an interactive console.
    /// </summary>
    [Fact]
    public async Task IngredientTreePromptCanBeDrivenWithoutAUser() {
        await database.ResetAsync();
        await using DB_FoodContext context = database.CreateContext();

        Food_DBEntity ingredient = new() { Name = "Apple", Energy_Kcal = 52 };
        Food_DBEntity composite = new() { Name = "Apple snack" };
        context.AddRange(ingredient, composite);
        await context.SaveChangesAsync();
        context.FoodIngredients.Add(new FoodIngredients_DBEntity {
            Food_Id = composite.Id,
            Ingredient_Food_Id = ingredient.Id,
            Weight_Ingredient_Normalised = 100m
        });
        await context.SaveChangesAsync();

        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        using StringReader input = new($"{composite.Id}{Environment.NewLine}");
        using StringWriter output = new();

        try {
            Console.SetIn(input);
            Console.SetOut(output);
            string? tree = await InteractiveMenu
                .GetFoodIngredientTreeInputAsync(context);

            Assert.NotNull(tree);
            Assert.Contains("Apple snack", tree);
            Assert.Contains("Apple", tree);
        } finally {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    /// <summary>
    /// Creates nutrient values for foods whose energy is relevant to a composite test.
    /// </summary>
    /// <param name="energy">Energy in kilocalories.</param>
    /// <returns>Nutrients with the supplied energy and zero for every other value.</returns>
    private static Nutrients CreateNutrients(double energy) => new(
        new Energy(energy),
        new Fat(),
        new Carbohydrates(),
        new Protein(),
        new Salt());
}