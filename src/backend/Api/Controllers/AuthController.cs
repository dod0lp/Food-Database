using Food_Database.Api.Contracts;
using Food_Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AuthRepository = Food_Database.Database.Repositories.Auth.Repository;

namespace Food_Database.Api.Controllers;

/// <summary>
/// Controller for handling authentication-related operations for user.
/// </summary>
[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase {
    /// <summary>
    /// The authentication repository.
    /// </summary>
    private readonly AuthRepository _authRepository;

    /// <summary>
    /// The sign-in manager for managing user sign-in operations using <see cref="Users_DBEntity"/>.
    /// </summary>
    private readonly SignInManager<Users_DBEntity> _signInManager;

    /// <summary>
    /// The user manager for managing user operations using <see cref="Users_DBEntity"/>.
    /// </summary>
    private readonly UserManager<Users_DBEntity> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class with the specified dependencies.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="signInManager">The sign-in manager.</param>
    /// <param name="userManager">The user manager.</param>
    public AuthController(
        AuthRepository authRepository,
        SignInManager<Users_DBEntity> signInManager,
        UserManager<Users_DBEntity> userManager) {
        _authRepository = authRepository;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Registers a new user with the provided email and password.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>The response containing the created user.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<CurrentUserResponse>> Register(RegisterRequest request) {
        if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password)) {
            return ValidationProblem("Email and password are required.");
        }

        IdentityResult result = await _authRepository.RegisterAsync(
            request.Email.Trim(), request.Password, HttpContext.RequestAborted);

        if (!result.Succeeded) {
            foreach (IdentityError error in result.Errors) {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(ModelState);
        }

        Users_DBEntity user =(await 
                _authRepository.GetByEmailAsync(request.Email.Trim()))!;

        return CreatedAtAction(nameof(Me),
                    new CurrentUserResponse(user.Id, user.Email!));
    }

    /// <summary>
    /// Authenticates a user with the provided email and password.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>The response containing the authenticated user information.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<CurrentUserResponse>> Login(LoginRequest request) {
        Users_DBEntity? user = await _authRepository.GetByEmailAsync(request.Email.Trim());
        if (user is null) {
            return Unauthorized();
        }

        Microsoft.AspNetCore.Identity.SignInResult result =
            await _signInManager.PasswordSignInAsync(
                user,
                request.Password,
                isPersistent: false,
                lockoutOnFailure: true);

        if (!result.Succeeded) {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse(user.Id, user.Email!));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout() {
        await _signInManager.SignOutAsync();

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me() {
        Users_DBEntity? user = await _userManager.GetUserAsync(User);

        return user is null
            ? Unauthorized()
            : Ok(new CurrentUserResponse(user.Id, user.Email!));
    }
}
