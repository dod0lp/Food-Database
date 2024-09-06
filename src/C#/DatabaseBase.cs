﻿using Food;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Food_Database_Base
{
    public static class DB_Descriptors
    {
        public static string MakeConnectionString(string server, string database, string user, string password, bool trustedServerCertificate = false)
        {
            string ret = $"Server={server};Database={database};User={user};Password={password};";
            
            if (trustedServerCertificate == true)
            {
                ret += "TrustServerCertificate = True;";
            }

            return ret;
        }
    }
    
    /// <summary>
    /// Class for using and making Food Database data, Accessing database, Making connection string for database connection [<see cref="ConnectionString"/>] <br></br>
    /// Constants for accessing database tables [starting with Table], Maximum lengths of variables [starting with Max],...
    /// </summary>
    public static class DB_Food_Descriptors
    {
        // TODO: Possibly make it so that those variables for food database are read from '.env' docker file
        // apparently not a good practice and this needs to be saved in launcsettings.json or similar
        private static readonly string server = "localhost";
        private static readonly string database = "db_food";
        private static readonly string user = "BackendCSharp";
        private static readonly string password = "Password@123";

        /// <summary>
        /// The connection string used to connect to the food database.
        /// </summary>
        /// <remarks>
        /// This string is constructed using the server, database, user, and password fields. It is utilized by the application to establish a connection to the database.
        /// <br/><br/>
        /// The connection string is generated by calling the <see cref="DB_Descriptors.MakeConnectionString"/> method, which formats the necessary parameters into a valid connection string.
        /// <br/><br/>
        /// Note: For security reasons, it is a good practice to store sensitive information such as database credentials in a secure location, for example stored in environmental variables
        /// storage, or a secure configuration service, rather than hardcoding them in the source code.
        /// </remarks>
        public static readonly string ConnectionString = DB_Descriptors.MakeConnectionString(server, database, user, password, true);

        /// <summary>
        /// The maximum allowed length for a food description in the database.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum number of characters allowed for the food description.
        /// If the description exceeds this length, it may be truncated or cause an error when saving to the database.
        /// </remarks>
        public const int MaxFoodDescriptionLength = 4_000;

        /// <summary>
        /// The maximum allowed length for a food name in the database.
        /// </summary>
        /// <remarks>
        /// This constant defines the maximum number of characters allowed for the food name.
        /// If the name exceeds this length, it may be truncated or cause an error when saving to the database.
        /// </remarks>
        public const int MaxFoodNameLength = 100;

        /// <summary>
        /// The name of the table in the database that stores food items.
        /// </summary>
        /// <remarks>
        /// This constant holds the name of the table where food data is stored without prefix of database name. It can be used to construct SQL queries and access food data.
        /// </remarks>
        public const string TableFood = "Food";

        /// <summary>
        /// The name of the table in the database that stores nutrient information.
        /// </summary>
        /// <remarks>
        /// This constant holds the name of the table where nutrient data is stored without prefix of database name. It can be used to construct SQL queries and access nutrient data.
        /// </remarks>
        public const string TableNutrients = "Nutrients";

        /// <summary>
        /// The name of the table in the database that stores food ingredients.
        /// </summary>
        /// <remarks>
        /// This constant holds the name of the table where ingredient data is stored without prefix of database name. It can be used to construct SQL queries and access ingredient data.
        /// </remarks>
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
        public double FatTotal { get; set; }

        [Column("Fat_Saturated")]
        public double FatSaturated { get; set; }

        [Column("Carbs_Total")]
        public double CarbsTotal { get; set; }

        [Column("Carbs_Saturated")]
        public double CarbsSaturated { get; set; }

        [Column("Protein_Total")]
        public double ProteinTotal { get; set; }

        [Column("Salt_Total")]
        public double SaltTotal { get; set; }


        // Navigation property to Food
        [ForeignKey("FoodId")]
        public FoodEntity Food { get; set; }
    }

    [Table(DB_Food_Descriptors.TableFood)]
    public class FoodEntity
    {
        [Key]
        [Column("ID")]
        public int FoodId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Weight")]
        public double Weight { get; set; }

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

    /// <summary>
    /// A <see cref="DbContext"/> instance for setting up a session and smooth workign with database.
    /// </summary>
    /// <remarks>
    /// - If configured correctly, will be good smooth experience using Model/Entity
    /// </remarks>
    public class FoodDbContext : DbContext
    {
        /// <summary>
        /// Food table entity, mapped so that I can add also <see cref="NutrientEntity"/> and/or <see cref="IngredientEntity"/> with this when
        /// aforementioned entities are defined,
        /// but also I can simply add only <see cref="FoodEntity"/>
        /// </summary>
        public DbSet<FoodEntity> Foods { get; set; }
        public DbSet<NutrientEntity> Nutrients { get; set; }
        public DbSet<IngredientEntity> Ingredients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(DB_Food_Descriptors.ConnectionString);
        }

        /// <summary>
        /// Configures entities and keys based on database relations defined in documentation of database in<br></br>
        /// entity relation diagram
        /// </summary>
        /// <param name="modelBuilder">Model builder instance</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IngredientEntity>()
                .HasKey(i => new { i.FoodIdComplete, i.FoodIdPart });

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

    /// <summary>
    /// Represents a class for mapping of Domain (C#) <see cref="Nutrients"/> to Entity (database) <see cref="NutrientEntity"/> models, and vice-versa.
    /// </summary>
    public static class NutrientMappingExtensions
    {
        /// <summary>
        /// Maps a <see cref="NutrientEntity"/> to a <see cref="Nutrients"/> domain model.
        /// </summary>
        /// <param name="entity">The <see cref="NutrientEntity"/> instance to be mapped.</param>
        /// <returns>A <see cref="Nutrients"/> domain model populated with data from the <paramref name="entity"/>.</returns>
        public static Nutrients MapToDomain(this NutrientEntity entity)
        {
            if (entity is null)
            {
                return new Nutrients(new(-1), new(-1, -1), new(-1, -1), new(-1), new(-1));
            }

            return new Nutrients(
                new Energy(entity.EnergyKcal),
                new Fat(entity.FatTotal, entity.FatSaturated),
                new Carbohydrates(entity.CarbsTotal, entity.CarbsSaturated),
                new Protein(entity.ProteinTotal),
                new Salt(entity.SaltTotal)
            );
        }

        /// <summary>
        /// Maps a <see cref="Nutrients"/> domain model to a <see cref="NutrientEntity"/> for database storage.
        /// </summary>
        /// <param name="model">The <see cref="Nutrients"/> domain model to be mapped.</param>
        /// <param name="foodId">The ID of the food item associated with this nutrient data. If database has autoincrement, this is not needed.</param>
        /// <returns>A <see cref="NutrientEntity"/> instance populated with data from the <paramref name="model"/>.</returns>
        /// <remarks>
        /// - The <see cref="EnergyKcal"/> and <see cref="EnergyKj"/> properties are cast to <c>int</c> as the database stores energy values as integers.
        /// - Other properties are cast to <c>double</c> to match the database schema for precision.
        /// - Ensure that the explicit casting does not result in data loss or precision issues.
        /// </remarks>
        public static NutrientEntity MapToEntity(this Nutrients model, int foodId = -1)
        {
            return new NutrientEntity
            {
                FoodId = foodId,
                // Energykcal is int in database, in Domain model it is double, but it is being explicitly cast as 'int'
                // when using setter
                EnergyKcal = (int)model.Energy.Kcal,
                EnergyKj = (int)model.Energy.KJ,

                FatTotal = (double)model.FatContent.Total,
                FatSaturated = (double)model.FatContent.Saturated,

                CarbsTotal = (double)model.CarbohydrateContent.Total,
                CarbsSaturated = (double)model.CarbohydrateContent.Sugar,

                ProteinTotal = (double)model.Protein.Total,
                
                SaltTotal = (double)model.Salt.Total
            };
        }
    }

    /// <summary>
    /// Represents a class for mapping of Domain (C#) <see cref="Food.Food"/> to Entity (database) <see cref="FoodEntity"/> models, and vice-versa.
    /// </summary>
    public static class FoodMappingExtensions
    {
        /// <summary>
        /// Maps a <see cref="FoodEntity"/> to a <see cref="Food.Food"/> domain model.
        /// </summary>
        /// <param name="entity">The <see cref="FoodEntity"/> instance to be mapped without ingredients list.</param>
        /// <returns>A <see cref="Food.Food"/> domain model populated with data from the <paramref name="entity"/>.</returns>
        public static Food.Food MapToDomainWithoutIngredients(this FoodEntity entity)
        {
            return
                new Food.Food(
                    entity.FoodId,
                    entity.Name,
                    entity.Weight,
                    entity.Nutrient.MapToDomain(),
                    entity.Description
                );
        }

        /// <summary>
        /// Maps a <see cref="FoodEntity"/> to a <see cref="Food.Food"/> domain model.
        /// </summary>
        /// <param name="entity">The <see cref="FoodEntity"/> instance to be mapped.</param>
        /// <returns>A <see cref="Food.Food"/> domain model populated with data from the <paramref name="entity"/>.</returns>
        public static Food.Food MapToDomain(this FoodEntity entity)
        {
            var ingredients = new List<Food.Food>();
            var retFood = entity.MapToDomainWithoutIngredients();

            if (entity.IngredientsAsComplete.Count > 0)
            {
                foreach (var ingredient in entity.IngredientsAsComplete)
                {
                    var temp = ingredient.FoodPart;
                    var domainIngredient = new Food.Food(
                        temp.FoodId,
                        temp.Name,
                        temp.Weight,
                        temp.Nutrient.MapToDomain(),
                        temp.Description
                    );

                    retFood.AddIngredient(domainIngredient);
                }
            }

            return retFood;
        }

        /// <summary>
        /// Maps a <see cref="Food.Food"/> domain model to a <see cref="FoodEntity"/> for database storage.
        /// </summary>
        /// <param name="model">The <see cref="Food.Food"/> domain model to be mapped.</param>
        /// <returns>A <see cref="FoodEntity"/> instance populated with data from the <paramref name="model"/>.</returns>
        /// <remarks>
        /// - The <see cref="Weight"/> property is cast to <c>double</c> to match the database schema, which may store weight as a floating-point number.
        /// - The <see cref="Nutrient"/> property is mapped using <see cref="Nutrients.MapToEntity(int)"/> and associates the food item ID with the nutrient data.
        /// - Ingredients are mapped to a set of <see cref="IngredientEntity"/> instances, with each ingredient linked back to the food entity.
        /// </remarks>
        public static FoodEntity MapToEntity(this Food.Food model)
        {
            var entity = new FoodEntity
            {
                // I can't add ID in this case, because it breaks database foreignkey,...
                Name = model.Name,
                Weight = (double)model.Weight,
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

    /// <summary>
    /// Class for retrieving/inserting database data
    /// </summary>
    public class DB_DataParser
    {
        /// <summary>
        /// The database context used for operations with the food database.
        /// </summary>
        /// <remarks>
        /// - This private readonly field holds the instance of <see cref="FoodDbContext"/> that is used to interact with the database.
        /// - It is initialized via the constructor and cannot be changed afterwards.
        /// </remarks>
        private readonly FoodDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DB_DataParser"/> class with the specified <see cref="FoodDbContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="FoodDbContext"/> instance to be used for database operations.</param>
        /// <remarks>
        /// - The provided <see cref="FoodDbContext"/> is assigned to the private field <see cref="context"/>.
        /// - This context is used for querying and saving <see cref="FoodEntity"/> instances to the database.
        /// </remarks>
        public DB_DataParser(FoodDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Retrieves all <see cref="FoodEntity"/> instances from the database, including related nutrient and ingredient data.
        /// </summary>
        /// <returns>A list of <see cref="FoodEntity"/> objects, each populated with its associated <see cref="NutrientEntity"/> and <see cref="IngredientEntity"/> details.</returns>
        /// <remarks>
        /// - The method uses <see cref="System.Linq"/> to eagerly load related data for nutrients and ingredients.
        /// - Ingredients are included using <see cref="System.Linq"/> and <see cref="Microsoft.EntityFrameworkCoreProperty"/> to include details about the ingredients (foods) associated with each <see cref="FoodEntity"/>.
        /// </remarks>
        public List<FoodEntity> GetAllFoodEntity()
        {
            return context.Foods
                .Include(f => f.Nutrient)  // Includes nutrients of this food
                .Include(f => f.IngredientsAsComplete)  // This includes ingredients (foods)
                    .ThenInclude(i => i.FoodPart)  // This includes the details of each ingredient
                .ToList();
        }

        /// <summary>
        /// Retrieves one <see cref="FoodEntity"/> instance from the database, including related nutrient and ingredient data, based on ID.
        /// </summary>
        /// <returns><see cref="FoodEntity"/>, or can return null if none found.</returns>
        public FoodEntity? GetFoodEntityById(int id)
        {
            return context.Foods.SingleOrDefault(f => f.FoodId == id);
        }

        /// <summary>
        /// Retrieves one <see cref="Food"/> instance from the database, casting from <see cref="FoodEntity"/>, including related nutrient and ingredient data, based on ID.
        /// </summary>
        /// <param name="id">ID of entity to lookup</param>
        /// <returns><see cref="Food"/>, or can return null if none found.</returns>
        public Food.Food? GetFoodDomainModelById(int id)
        {
            var foodEntity = GetFoodEntityById(id);

            if (foodEntity == null)
            {
                return null;
            }

            return foodEntity.MapToDomain();
        }

        /// <summary>
        /// Inserts a new <see cref="FoodEntity"/> into the database, created from the provided <see cref="Food"/> domain model.
        /// </summary>
        /// <param name="food">The <see cref="Food"/> domain model to be inserted into the database.</param>
        /// <returns>The primary key ID of the newly inserted <see cref="FoodEntity"/>.</returns>
        /// <remarks>
        /// - The method maps the provided <see cref="Food"/> domain model to a <see cref="FoodEntity"/> using <see cref="Food.Food.MapToEntity"/>.
        /// - The <see cref="FoodEntity"/> is then added to the database context and saved using <see cref="Microsoft.EntityFrameworkCore"/>.
        /// - The ID of the newly inserted entity is returned, which corresponds to the database-generated primary key.
        /// </remarks>
        public int InsertFoodFromDomain(Food.Food food)
        {
            FoodEntity foodEntity = food.MapToEntity();
            context.Foods.Add(foodEntity);
            context.SaveChanges();
            return foodEntity.FoodId;
        }

        /// <summary>
        /// Inserts a new <see cref="IngredientEntity"/> into the database, created from the provided <see cref="Food.Id"/> domain model.
        /// </summary>
        /// <param name="id">he primary key ID of the newly inserted <see cref="FoodEntity"/>. Turning into the database as 1:1 mapping.</param>
        /// <remarks>
        /// - The method maps the provided <see cref="Food.Id"/> domain model to a <see cref="Food.Id"/> using <see cref="Food.Food.MapToEntity"/>.
        /// </remarks>
        public void InsertFoodMappings(int id)
        {
            List<int> listPartIds = GetFoodPartIdsByCompleteFoodId(id);
            var ingredients = new List<IngredientEntity>();

            foreach (var partId in listPartIds)
            {
                ingredients.Add(new IngredientEntity
                {
                    FoodIdComplete = id,
                    FoodIdPart = partId
                });
            }
            ingredients.Add(new IngredientEntity
            {
                FoodIdComplete = id,
                FoodIdPart = id
            });

            context.Ingredients.AddRange(ingredients);
            context.SaveChanges();
        }

        /// <summary>
        /// Gets IDs of all pairs that this <paramref name="foodIdComplete"/> is CompleteID
        /// </summary>
        /// <param name="foodIdComplete">Complete ID of which I want to have all foods it's made out of</param>
        /// <returns></returns>
        public List<int> GetFoodPartIdsByCompleteFoodId(int foodIdComplete)
        {
            var x = context.Foods
                .Where(f => f.FoodId == foodIdComplete)
                .Include(f => f.IngredientsAsComplete)
                    .ThenInclude(i => i.FoodPart)
                .SelectMany(f => f.IngredientsAsComplete.Select(i => i.FoodPart.FoodId))
                .ToList();

            return x;
        }
    }
}
