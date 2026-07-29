using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Food_Database.Base;

namespace Food_Database.Base.Models;

[Table(DB_Food_Descriptors.Table.FoodIngredients)]
public partial class FoodIngredients_DBEntity
{
    public int Food_Id { get; set; }

    public int Ingredient_Food_Id { get; set; }

    public decimal Weight_Ingredient_Normalised { get; set; }

    public virtual Food_DBEntity Food { get; set; } = null!;

    public virtual Food_DBEntity Ingredient_Food { get; set; } = null!;
}