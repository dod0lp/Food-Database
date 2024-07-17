namespace Food_Database_Base
{
    public static class DB_Descriptors
    {
        private static string _server = "localhost";
        private static string _user = "user";
        private static string _password = "pw";

        public static string connectionString = $"Server={_server};Database=db_food;User={_user};Password={_password};";

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
