using Food_Database.Database.Descriptors;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food_Database.Models;

[Table(DB_Food_Descriptors.Table.UserFoodOptions)]
public partial class UserFoodOptions_DBEntity {
    public int User_Id { get; set; }

    public int Food_Id { get; set; }

    public decimal Weight_Total { get; set; }

    public decimal? Price_Eur { get; set; }

    public virtual Food_DBEntity Food { get; set; } = null!;

    public virtual Users_DBEntity User { get; set; } = null!;
}