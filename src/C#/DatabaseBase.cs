namespace Food_Database_Base
{
    public static class DB_Descriptors
    {
        private static readonly string _server = "localhost";
        private static readonly string _database = "db_food";
        private static readonly string _user = "user";
        private static readonly string _password = "pw";

        private static readonly string connectionString = $"Server={_server};Database={_database};User={_user};Password={_password};";

        // Needs to be const because [Descriptor] this thing is used for needs const value
        public const int _max_food_description_length = 10_000;
    }

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
