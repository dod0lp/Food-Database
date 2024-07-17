namespace Food_Database_Base
{
    public static class DB_Descriptors
    {
        private static string _server = "localhost";
        private static string _user = "user";
        private static string _password = "pw";
        
        public static string connectionString = $"Server={_server};Database=db_food;User={_user};Password={_password};";
        public const int _max_food_description_length = 10_000;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
        }
    }
}
