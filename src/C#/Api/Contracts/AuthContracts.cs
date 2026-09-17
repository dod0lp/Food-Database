namespace Food_Database.Api.Contracts;

/// <summary>
/// Request to register a new user.
/// </summary>
/// <param name="Email">User email.</param>
/// <param name="Password">User password.</param>
public sealed record RegisterRequest(string Email, string Password);


/// <summary>
/// Request to login an existing user.
/// </summary>
/// <param name="Email">User email.</param>
/// <param name="Password">User password.</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Response containing the current authenticated user's information.
/// </summary>
/// <param name="Id">ID of a user.</param>
/// <param name="Email">Email of a user.</param>
public sealed record CurrentUserResponse(int Id, string Email);
