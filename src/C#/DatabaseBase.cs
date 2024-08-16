using Food;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Food_Database_Base
{
    public static class DB_Descriptors
    {
        public static string MakeConnectionString(string server, string database, string user, string password)
        {
            return $"Server={server};Database={database};User={user};Password={password};";
        }
    }

    public static class DB_Food_Descriptors
    {
        // TODO: Possibly make it so that those variables for food database are read from '.env' docker file
        // apparently not a good practice and this needs to be saved in launcsettings.json or similar
        private static readonly string server = "localhost";
        private static readonly string database = "db_food";
        private static readonly string user = "BackendCSharp";
        private static readonly string password = "Password@123";

        public static readonly string connectionString = DB_Descriptors.MakeConnectionString(server, database, user, password);

        // Needs to be const because [Descriptor] this thing is used for needs const value
        public const int maxFoodDescriptionLength = 4_000;
        public const int maxFoodNameLength = 100;

        public const string tableFood = "Food";
        public const string tableNutrients = "Nutrients";
        public const string tableFoodIngredients = "Ingredients";
    }

    // TODO: MapToDomain & MapToEntity for those classes

    [Table(DB_Food_Descriptors.tableNutrients)]
    public class NutrientEntity
    {
        [Key]
        [Column("Food_ID")]
        public int FoodId { get; set; }

        [Column("Energy_Kcal")]
        public int EnergyKcal { get; set; }

        [Column("Fat_Total")]
        public float FatTotal { get; set; }

        // Navigation property to Food
        [ForeignKey("FoodId")]
        public FoodEntity Food { get; set; }
    }

    [Table(DB_Food_Descriptors.tableFood)]
    public class FoodEntity
    {
        [Key]
        [Column("Food_ID")]
        public int FoodId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Weight")]
        public float Weight { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        // Navigation property to Nutrient
        public NutrientEntity Nutrient { get; set; }

        // Navigation properties for Ingredients
        public ICollection<IngredientEntity> IngredientsAsPart { get; set; } = new HashSet<IngredientEntity>();
        public ICollection<IngredientEntity> IngredientsAsComplete { get; set; } = new HashSet<IngredientEntity>();
    }

    [Table(DB_Food_Descriptors.tableFoodIngredients)]
    public class IngredientEntity
    {
        [Key]
        [Column("Food_ID_Complete")]
        public int FoodIdComplete { get; set; }

        [Key]
        [Column("Food_ID_Part")]
        public int FoodIdPart { get; set; }

        // Navigation properties
        [ForeignKey("FoodIdComplete")]
        public FoodEntity FoodComplete { get; set; }

        [ForeignKey("FoodIdPart")]
        public FoodEntity FoodPart { get; set; }
    }



    // TODO: create a C# backend (ASP.NET Core Web API) that uses EntityFramework or ADO.NET to interact with the database.

    // TODO: Make descriptor for this very database using Food table name, class description etc.
    // Or maybe just use LinqToSql to my predefined Food class with structs etc

    /*class Program_Database
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
        }
    }*/
}
