using Food_Database.Api.Contracts;
using Food_Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AuthRepository = Food_Database.Database.Repositories.Auth.Repository;

namespace Food_Database.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase {
    private readonly AuthRepository _authRepository;
    private readonly SignInManager<Users_DBEntity> _signInManager;
    private readonly UserManager<Users_DBEntity> _userManager;

    public AuthController(
        AuthRepository authRepository,
        SignInManager<Users_DBEntity> signInManager,
        UserManager<Users_DBEntity> userManager) {
        _authRepository = authRepository;
        _signInManager = signInManager;
        _userManager = userManager;
    }

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

        Users_DBEntity user = (await _authRepository.GetByEmailAsync(request.Email.Trim()))!;
        return CreatedAtAction(nameof(Me), new CurrentUserResponse(user.Id, user.Email!));
    }

    [HttpPost("login")]
    public async Task<ActionResult<CurrentUserResponse>> Login(LoginRequest request) {
        Users_DBEntity? user = await _authRepository.GetByEmailAsync(request.Email.Trim());
        if (user is null) {
            return Unauthorized();
        }

        Microsoft.AspNetCore.Identity.SignInResult result =
            await _signInManager.PasswordSignInAsync(
            user, request.Password, isPersistent: false, lockoutOnFailure: true);

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
