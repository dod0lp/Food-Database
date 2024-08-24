using Food;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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

        public static readonly string ConnectionString = DB_Descriptors.MakeConnectionString(server, database, user, password);

        // Needs to be const because [Descriptor] this thing is used for needs const value
        public const int MaxFoodDescriptionLength = 4_000;
        public const int MaxFoodNameLength = 100;

        public const string TableFood = "Food";
        public const string TableNutrients = "Nutrients";
        public const string TableFoodIngredients = "Ingredients";
    }

    // TODO: MapToDomain & MapToEntity for those classes

    [Table(DB_Food_Descriptors.TableNutrients)]
    public class NutrientEntity
    {
        [Key]
        [Column("Food_ID")]
        public int FoodId { get; set; }

        [Column("Energy_Kcal")]
        public int EnergyKcal { get; set; }

        [Column("Energy_Kj")]
        public int EnergyKj { get; set; }

        [Column("Fat_Total")]
        public float FatTotal { get; set; }

        [Column("Fat_Saturated")]
        public float FatSaturated { get; set; }

        [Column("Carbs_Total")]
        public float CarbsTotal { get; set; }

        [Column("Carbs_Saturated")]
        public float CarbsSaturated { get; set; }

        [Column("Protein_Total")]
        public float ProteinTotal { get; set; }

        [Column("Salt_Total")]
        public float SaltTotal { get; set; }


        // Navigation property to Food
        [ForeignKey("FoodId")]
        public FoodEntity Food { get; set; }
    }

    [Table(DB_Food_Descriptors.TableFood)]
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

    [Table(DB_Food_Descriptors.TableFoodIngredients)]
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

    public class FoodDbContext : DbContext
    {
        public DbSet<NutrientEntity> Nutrients { get; set; }
        public DbSet<FoodEntity> Foods { get; set; }
        public DbSet<IngredientEntity> Ingredients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(DB_Food_Descriptors.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IngredientEntity>()
                .HasKey(i => new { i.FoodIdComplete, i.FoodIdPart });

            // Simply configure key relations
            modelBuilder.Entity<NutrientEntity>()
                .HasOne(n => n.Food)
                .WithOne(f => f.Nutrient)
                .HasForeignKey<NutrientEntity>(n => n.FoodId);

            modelBuilder.Entity<FoodEntity>()
                .HasMany(f => f.IngredientsAsPart)
                .WithOne(i => i.FoodPart)
                .HasForeignKey(i => i.FoodIdPart);

            modelBuilder.Entity<FoodEntity>()
                .HasMany(f => f.IngredientsAsComplete)
                .WithOne(i => i.FoodComplete)
                .HasForeignKey(i => i.FoodIdComplete);
        }
    }

    public static class NutrientMappingExtensions
    {
        // simple Mapping from Entity to Domain
        public static Nutrients MapToDomain(this NutrientEntity entity)
        {
            return new Nutrients(
                new Energy(entity.EnergyKcal),
                new Fat(entity.FatTotal, entity.FatSaturated),
                new Carbohydrates(entity.CarbsTotal, entity.CarbsSaturated),
                new Protein(entity.ProteinTotal),
                new Salt(entity.SaltTotal)
            );
        }

        // Maping from Domain to Entity model
        // it is explicitly casted as float, because in DB it is float
        // hope this wont break anything
        public static NutrientEntity MapToEntity(this Nutrients model, int foodId)
        {
            return new NutrientEntity
            {
                FoodId = foodId,
                // Energykcal is int in database, in Domain model it is float, but it is being explicitly cast as 'int'
                // when using setter
                EnergyKcal = (int)model.Energy.Kcal,
                EnergyKj = (int)model.Energy.KJ,

                FatTotal = (float)model.FatContent.Total,
                FatSaturated = (float)model.FatContent.Saturated,

                CarbsTotal = (float)model.CarbohydrateContent.Total,
                CarbsSaturated = (float)model.CarbohydrateContent.Sugar,

                ProteinTotal = (float)model.Protein.Total,
                
                SaltTotal = (float)model.Salt.Total
            };
        }
    }

    public static class FoodMappingExtensions
    {
        // Map FoodEntity to Food (Domain)
        public static Food.Food MapToDomain(this FoodEntity entity)
        {
            return new Food.Food(
                entity.FoodId,
                entity.Name,
                entity.Weight,
                entity.Nutrient.MapToDomain(),
                entity.Description,
                entity.IngredientsAsComplete.Select(i => i.FoodPart.MapToDomain()).ToList()
            );
        }

        // Map Food (Domain) to FoodEntity
        public static FoodEntity MapToEntity(this Food.Food model)
        {
            var entity = new FoodEntity
            {
                FoodId = model.Id,
                Name = model.Name,
                Weight = (float)model.Weight,
                Description = model.Description,
                Nutrient = model.NutrientContent.MapToEntity(model.Id),
                IngredientsAsPart = new HashSet<IngredientEntity>()
            };

            // Map Ingredients
            if (model.Ingredients != null)
            {
                foreach (var ingredient in model.Ingredients)
                {
                    entity.IngredientsAsPart.Add(new IngredientEntity
                    {
                        FoodIdComplete = model.Id,
                        FoodIdPart = ingredient.Id,
                        FoodComplete = entity,
                        FoodPart = ingredient.MapToEntity()
                    });
                }
            }

            return entity;
        }
    }


    // TODO: create a C# backend (ASP.NET Core Web API) that uses EntityFramework interact with the database.

    /*class Program_Database
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
        }
    }*/
}
