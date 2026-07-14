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

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.SleeperUserId),
            new(ClaimTypes.Email, user.Email ?? "")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
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

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.SleeperUserId), new(ClaimTypes.Email, user.Email ?? "") };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
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

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
}
