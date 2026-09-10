using Food_Database.Models;
using Microsoft.AspNetCore.Identity;

namespace Food_Database.Database.Repositories.Auth;

/// <summary>
/// Identity-backed access to application users.
/// Password hashing and validation are deliberately delegated to ASP.NET Core
/// Identity; this repository never reads or stores a password itself.
/// </summary>
public sealed class Repository {
    private readonly UserManager<Users_DBEntity> _users;

    public Repository(UserManager<Users_DBEntity> users) {
        _users = users;
    }

    public Task<Users_DBEntity?> GetByIdAsync(int userId) =>
        _users.FindByIdAsync(userId.ToString());

    public Task<Users_DBEntity?> GetByEmailAsync(string email) =>
        _users.FindByEmailAsync(email);

    public async Task<IdentityResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default) {
        var user = new Users_DBEntity {
            UserName = email,
            Email = email
        };

        return await _users.CreateAsync(user, password);
    }
}
