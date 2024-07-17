namespace Food_Database_Base
{
    public static class DB_Descriptors
    {
        public static string connectionString = "Server=localhost;Database=db_food;User=user;Password=pw;";
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
