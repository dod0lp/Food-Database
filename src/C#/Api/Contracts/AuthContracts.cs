namespace Food_Database.Api.Contracts;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record CurrentUserResponse(int Id, string Email);
