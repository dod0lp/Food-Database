using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Food_Database.Database.Descriptors;

namespace Food_Database.Models;

[Table(DB_Food_Descriptors.Table.Food)]
public partial class Food_DBEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Food_Description { get; set; }

    public int? Energy_Kcal { get; set; }

    public decimal? Fat_Total { get; set; }

    public decimal? Fat_Saturated { get; set; }

    public decimal? Carbs_Total { get; set; }

    public decimal? Carbs_Sugar { get; set; }

    public decimal? Protein_Total { get; set; }

    public decimal? Salt_Total { get; set; }

    public virtual ICollection<FoodIngredients_DBEntity> FoodIngredientsFood { get; set; } = new List<FoodIngredients_DBEntity>();

    public virtual ICollection<FoodIngredients_DBEntity> FoodIngredientsIngredient_Food { get; set; } = new List<FoodIngredients_DBEntity>();

    public virtual UserCreatedFood_DBEntity? UserCreatedFood { get; set; }

    public virtual ICollection<UserFoodOptions_DBEntity> UserFoodOptions { get; set; } = new List<UserFoodOptions_DBEntity>();

    public virtual ICollection<UserFoodRemarks_DBEntity> UserFoodRemark { get; set; } = new List<UserFoodRemarks_DBEntity>();

    public virtual ICollection<Users_DBEntity> User { get; set; } = new List<Users_DBEntity>();
}