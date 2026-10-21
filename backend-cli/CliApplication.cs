using Food_Database.Database.Descriptors;
using Food_Database.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ProgramTestFood;

/// <summary>
/// Starts the CLI session and handles failures while connecting to SQL Server.
/// </summary>
internal static class CliApplication {
    /// <summary>
    /// Runs one interactive session and returns a shell-friendly exit code.
    /// </summary>
    internal static async Task<int> RunAsync() {
        try {
            await RunInteractiveSessionAsync();
            return 0;
        } catch (Exception exception)
            when (IsDatabaseException(exception)) {
            Console.Error.WriteLine(
                "Database operation failed. " +
                "If this failed at the start, check if SQL Server is running " +
                "and that the connection settings are correct.");
            Console.Error.WriteLine($"Details: {exception.GetBaseException().Message}");
            return 1;
        }
    }

    /// <summary>
    /// Determines whether an exception was caused by a database operation.
    /// </summary>
    private static bool IsDatabaseException(Exception exception) =>
        exception is SqlException or DbUpdateException or TimeoutException ||
        exception.InnerException is not null &&
            IsDatabaseException(exception.InnerException);

    /// <summary>
    /// Connects to the database, selects a user, and starts the menu.
    /// </summary>
    private static async Task RunInteractiveSessionAsync() {
        DbContextOptions<DB_FoodContext> options =
            new DbContextOptionsBuilder<DB_FoodContext>()
                .UseSqlServer(
                    DB_Food_Descriptors.ConnectionString,
                    sqlOptions => sqlOptions.EnableRetryOnFailure())
                .Options;

        using DB_FoodContext db = new(options);

        Console.Write("User ID: ");
        if (!int.TryParse(Console.ReadLine(), out int userId)) {
            Console.WriteLine("Enter a valid numeric user ID.");
            return;
        }

        Users_DBEntity? user = await db.Users
            .Include(x => x.Food)
            .SingleOrDefaultAsync(x => x.Id == userId);

        if (user is null) {
            Console.WriteLine("User ID doesn't exist in database. Exiting.");
            return;
        }

        Console.WriteLine("User is logged in as user ID: " + userId);
        await InteractiveMenu.RunAsync(db, userId);
    }
}