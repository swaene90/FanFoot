using System.Security.Claims;
using Fanfoot.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using Fanfoot.Web.Api.Dtos;

namespace Fanfoot.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly IAntiforgery _antiforgery;

    public AuthController(AuthService auth, IAntiforgery antiforgery)
    {
        _auth = auth;
        _antiforgery = antiforgery;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _auth.ValidateCredentialsAsync(request.Email, request.Password);
        if (user == null)
            return Unauthorized();

        await SignInAsync(user);
        return Ok(new LoginResponse(new AuthUserDto(user.SleeperUserId, null, user.Email)));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request.Email, request.Password, request.SleeperUsername);
        if (!result.Success)
            return BadRequest(new { error = result.Error });

        var user = await _auth.ValidateCredentialsAsync(request.Email, request.Password);
        if (user == null)
            return Problem("The account was created but could not be signed in.");

        await SignInAsync(user);
        return Ok(new LoginResponse(new AuthUserDto(user.SleeperUserId, null, user.Email)));
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return id == null ? Unauthorized() : Ok(new AuthUserDto(id, null, User.FindFirstValue(ClaimTypes.Email)));
    }

    [HttpGet("antiforgery")]
    public IActionResult Antiforgery()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    [HttpPost("password-reset/request")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestPasswordReset(PasswordResetRequest request, CancellationToken cancellationToken)
    {
        await _auth.RequestPasswordResetAsync(request.Email, cancellationToken);
        return Ok(new { message = "If an account exists for that email, a password reset link has been sent." });
    }

    [HttpPost("password-reset/confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPasswordReset(PasswordResetConfirmRequest request, CancellationToken cancellationToken)
    {
        var result = await _auth.ResetPasswordAsync(request.Token, request.Password, cancellationToken);
        return result.Success ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    private async Task SignInAsync(Fanfoot.Domain.Models.LocalUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.SleeperUserId),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("session_version", user.SessionVersion.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
