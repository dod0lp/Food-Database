using Microsoft.EntityFrameworkCore;

using Food_Database.Models;

namespace Food_Database.Database.Repositories.Foods {
    /// <summary>
    /// Class to work with database Food.
    /// </summary>
    public sealed partial class Repository {
        /// <summary>
        /// <see cref="DbContext"/> for food database.
        /// </summary>
        private readonly DB_FoodContext _db;

        /// <summary>
        /// Sets <see cref="DbContext"/>.
        /// </summary>
        /// <param name="db">Database connection.</param>
        public Repository(DB_FoodContext db) {
            _db = db;
        }

        /// <summary>
        /// Public call to save database context changes.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for async op.</param>
        /// <returns>Empty <see cref="Task"/>.</returns>
        public async Task SaveChangesDBAsync(CancellationToken cancellationToken = default) {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
