using Food_Database;
using Food;
using Food_Database.Database.Operations;
using Food_Database.Descriptors;
using Food_Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public static class Program_Food
{
    private const int UserId = 1;

    public static void Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<DB_FoodContext>()
            .UseSqlServer(DB_Food_Descriptors.ConnectionString)
            .Options;

        using var db = new DB_FoodContext(options);

        EnsureDummyData(db);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("1 - Browse foods");
            Console.WriteLine("2 - Get food by ID");
            Console.WriteLine("3 - Create new food and favorite it");
            Console.WriteLine("4 - Favorite existing food");
            Console.WriteLine("5 - Show my favorites");
            Console.WriteLine("0 - Exit");
            Console.WriteLine("==============================================");
            Console.Write("> ");

            switch (Console.ReadLine())
            {
                case "1":
                    BrowseFoods(db);
                    break;

                case "2":
                    GetFoodById(db);
                    break;

                case "3":
                    CreateFoodAndFavorite(db, UserId);
                    break;

                case "4":
                    FavoriteExistingFood(db, UserId);
                    break;

                case "5":
                    ShowFavorites(db, UserId);
                    break;

                case "0":
                    return;
            }
        }
    }

    private static void EnsureDummyData(DB_FoodContext db)
    {
        if (!db.Users.Any(x => x.Id == UserId))
        {
            var user = new Users_DBEntity();
            db.Users.Add(user);
            db.SaveChanges();
        }

        if (db.Food.Any())
        {
            return;
        }

        db.Food.AddRange(
            new Food_DBEntity
            {
                Name = "Chicken Breast",
                Food_Description = "Chicken breast",
                Energy_Kcal = 165,
                Fat_Total = 3.6m,
                Fat_Saturated = 1m,
                Carbs_Total = 0,
                Carbs_Sugar = 0,
                Protein_Total = 31m,
                Salt_Total = 0.2m
            },

            new Food_DBEntity
            {
                Name = "Rice",
                Food_Description = "Cooked white rice",
                Energy_Kcal = 130,
                Fat_Total = 0.3m,
                Fat_Saturated = 0.1m,
                Carbs_Total = 28m,
                Carbs_Sugar = 0.1m,
                Protein_Total = 2.7m,
                Salt_Total = 0.01m
            },

            new Food_DBEntity
            {
                Name = "Egg",
                Food_Description = "Whole egg",
                Energy_Kcal = 155,
                Fat_Total = 11m,
                Fat_Saturated = 3.3m,
                Carbs_Total = 1.1m,
                Carbs_Sugar = 1.1m,
                Protein_Total = 13m,
                Salt_Total = 0.31m
            }
        );

        db.SaveChanges();
    }

    private static void BrowseFoods(DB_FoodContext db)
    {
        var foods = db.Food
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToList();

        Console.WriteLine();

        foreach (var food in foods)
        {
            Console.WriteLine($"{food.Id}: {food.Name}");
        }
    }

    private static void GetFoodById(DB_FoodContext db)
    {
        Console.Write("Food ID: ");

        if (!int.TryParse(Console.ReadLine(), out int foodId))
            return;

        Food_DBEntity? entity = db.Food
            .AsNoTracking()
            .SingleOrDefault(x => x.Id == foodId);

        if (entity is null)
        {
            Console.WriteLine("Food not found.");
            return;
        }

        Food.Food food = entity.ToDomain();

        Console.WriteLine();
        Console.WriteLine($"ID:          {food.Id}");
        Console.WriteLine($"Name:        {food.Name}");
        Console.WriteLine($"Weight:      {food.Weight} g");
        Console.WriteLine($"Description: {food.Description}");
    }

    private static void CreateFoodAndFavorite(
        DB_FoodContext db,
        int userId)
    {
        Users_DBEntity? user = db.Users
            .Include(x => x.Food)
            .SingleOrDefault(x => x.Id == userId);

        if (user is null)
            return;

        Console.Write("Food name: ");
        string? name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
            return;

        Console.Write("Energy kcal / 100g: ");
        int.TryParse(Console.ReadLine(), out int kcal);

        Console.Write("Fat / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal fat);

        Console.Write("Saturated fat / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal saturated);

        Console.Write("Carbs / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal carbs);

        Console.Write("Sugar / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal sugar);

        Console.Write("Protein / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal protein);

        Console.Write("Salt / 100g: ");
        decimal.TryParse(Console.ReadLine(), out decimal salt);

        var newFood = new Food_DBEntity
        {
            Name = name,
            Energy_Kcal = kcal,
            Fat_Total = fat,
            Fat_Saturated = saturated,
            Carbs_Total = carbs,
            Carbs_Sugar = sugar,
            Protein_Total = protein,
            Salt_Total = salt
        };

        // First create the actual Food row so SQL generates its ID.
        db.Food.Add(newFood);
        db.SaveChanges();

        // Mark this food as created by this user.
        db.UserCreatedFood.Add(new UserCreatedFood_DBEntity
        {
            Food_Id = newFood.Id,
            User_Id = userId
        });

        // Also make it this user's favorite.
        user.Food.Add(newFood);

        db.SaveChanges();

        Console.WriteLine(
            $"Created food {newFood.Id}: {newFood.Name} and added to favorites.");
    }

    private static void FavoriteExistingFood(
        DB_FoodContext db,
        int userId)
    {
        BrowseFoods(db);

        Console.WriteLine();
        Console.Write("Food ID to favorite: ");

        if (!int.TryParse(Console.ReadLine(), out int foodId))
            return;

        Users_DBEntity? user = db.Users
            .Include(x => x.Food)
            .SingleOrDefault(x => x.Id == userId);

        if (user is null)
            return;

        Food_DBEntity? food = db.Food
            .SingleOrDefault(x => x.Id == foodId);

        if (food is null)
        {
            Console.WriteLine("Food not found.");
            return;
        }

        if (user.Food.Any(x => x.Id == foodId))
        {
            Console.WriteLine("Food is already a favorite.");
            return;
        }

        user.Food.Add(food);

        db.SaveChanges();

        Console.WriteLine(
            $"Added {food.Id}: {food.Name} to favorites.");
    }

    private static void ShowFavorites(
        DB_FoodContext db,
        int userId)
    {
        Users_DBEntity? user = db.Users
            .AsNoTracking()
            .Include(x => x.Food)
            .SingleOrDefault(x => x.Id == userId);

        if (user is null)
            return;

        Console.WriteLine();

        foreach (Food_DBEntity food in user.Food.OrderBy(x => x.Id))
        {
            Console.WriteLine($"{food.Id}: {food.Name}");
        }
    }
}