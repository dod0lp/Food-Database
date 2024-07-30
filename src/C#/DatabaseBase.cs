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
        private static readonly string server = "localhost";
        private static readonly string database = "db_food";
        private static readonly string user = "BackendCSharp";
        private static readonly string password = "Password@123";

        public static readonly string connectionString = DB_Descriptors.MakeConnectionString(server, database, user, password);

        // Needs to be const because [Descriptor] this thing is used for needs const value
        public const int maxFoodDescriptionLength = 4_000;
        public const int maxFoodNameLength = 100;

        private static readonly string tableFood = "Food";
        private static readonly string tableNutrients = "Nutrients";
        private static readonly string tableFoodIngredients = "Ingredients";
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
