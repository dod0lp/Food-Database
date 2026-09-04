using Food_Database.Database.Descriptors;
using Food_Database.Database.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food_Database.Database.Models.Food;

[Table(DB_Food_Descriptors.Table.UserFoodRemarks)]
public partial class UserFoodRemarks_DBEntity {
    public int User_Id { get; set; }

    public int Food_Id { get; set; }

    public string? Food_Remark { get; set; }

    public virtual Food_DBEntity Food { get; set; } = null!;

    public virtual Users_DBEntity User { get; set; } = null!;
}