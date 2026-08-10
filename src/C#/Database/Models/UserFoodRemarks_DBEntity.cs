using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Food_Database.Database.Descriptors;

namespace Food_Database.Models;

[Table(DB_Food_Descriptors.Table.UserFoodRemarks)]
public partial class UserFoodRemarks_DBEntity
{
    public int User_Id { get; set; }

    public int Food_Id { get; set; }

    public string? Food_Remark { get; set; }

    public virtual Food_DBEntity Food { get; set; } = null!;

    public virtual Users_DBEntity User { get; set; } = null!;
}