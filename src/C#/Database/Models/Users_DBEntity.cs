using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Food_Database.Descriptors;

namespace Food_Database.Models;

[Table(DB_Food_Descriptors.Table.Users)]
public partial class Users_DBEntity
{
    public int Id { get; set; }

    public virtual ICollection<UserCreatedFood_DBEntity> UserCreatedFood { get; set; } = new List<UserCreatedFood_DBEntity>();

    public virtual ICollection<UserFoodOptions_DBEntity> UserFoodOptions { get; set; } = new List<UserFoodOptions_DBEntity>();

    public virtual ICollection<UserFoodRemarks_DBEntity> UserFoodRemark { get; set; } = new List<UserFoodRemarks_DBEntity>();

    public virtual ICollection<Food_DBEntity> Food { get; set; } = new List<Food_DBEntity>();
}