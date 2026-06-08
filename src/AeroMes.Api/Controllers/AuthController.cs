using AeroMes.Application.Interfaces;
using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Email, request.Password,
            isPersistent: false, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return StatusCode(StatusCodes.Status429TooManyRequests,
                new { message = "Account locked due to too many failed attempts. Try again later." });

        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password." });

        var user = await userManager.FindByEmailAsync(request.Email);
        var roles = await userManager.GetRolesAsync(user!);
        var token = tokenService.CreateToken(user!.Id, user.Email!, roles);

        return Ok(new LoginResponse(token, user.Email!, user.FullName, [.. roles]));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new { user.Id, user.Email, user.FullName, Roles = roles });
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string FullName, string[] Roles);
