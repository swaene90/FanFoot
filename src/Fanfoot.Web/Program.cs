using Fanfoot.Infrastructure;
using MudBlazor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Fanfoot.Domain;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Services;
using Fanfoot.Web.Components;
using Fanfoot.Web.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeysPath"] ?? "keys"));

builder.Services.AddMudServices();
builder.Services.AddScoped<IPasswordHasher<LocalUser>, PasswordHasher<LocalUser>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=fanfoot.db";
builder.Services.AddFanfootInfrastructure(connectionString);
builder.Services.AddHostedService<PlayerImportService>();
builder.Services.AddScoped<ChatService>();
var groqApiKey = builder.Configuration["GroqApiKey"];
if (!string.IsNullOrEmpty(groqApiKey))
{
    builder.Services.AddHttpClient("Ollama", client =>
    {
        client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", groqApiKey);
        client.Timeout = TimeSpan.FromSeconds(60);
    });
}
else
{
    var ollamaUrl = builder.Configuration["OllamaUrl"] ?? "http://192.168.0.48:11434/";
    builder.Services.AddHttpClient("Ollama", client =>
    {
        client.BaseAddress = new Uri(new Uri(ollamaUrl), "v1/");
        client.Timeout = TimeSpan.FromSeconds(120);
    });
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<FanfootDbContext>();
        db.Database.Migrate();

        if (app.Environment.IsDevelopment())
            await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database migration skipped — no database connection");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapOpenApi();

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    FanfootDbContext db,
    IPasswordHasher<LocalUser> hasher,
    HttpContext ctx) =>
{
    var user = await db.LocalUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user?.PasswordHash == null)
        return Results.Unauthorized();

    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
    if (result == PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.SleeperUserId),
        new(ClaimTypes.Email, user.Email ?? "")
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    return Results.Ok();
});

app.MapPost("/api/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
}).RequireAuthorization();

app.MapPost("/api/players/import", async (LeagueService leagueService) =>
{
    var count = await leagueService.ImportPlayersAsync();
    return Results.Ok(new { imported = count });
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

record LoginRequest(string Email, string Password);
