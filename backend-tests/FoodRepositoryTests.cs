using Food_Database.Database.Repositories.Foods;
using Food_Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDatabase.Tests;

[Collection(DatabaseCollection.Name)]
public sealed class FoodRepositoryTests(SqlServerDatabaseFixture database) {
    [Fact]
    public async Task NewDatabaseHasAnEmptyFoodTable() {
        await using DB_FoodContext context = database.CreateContext();
        await context.Food.ExecuteDeleteAsync();

        Assert.Equal(0, await context.Food.CountAsync());
    }

    [Fact]
    public async Task GetFoodsReturnsPersistedFoodsInIdOrder() {
        await using DB_FoodContext context = database.CreateContext();
        await context.Food.ExecuteDeleteAsync();

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
}