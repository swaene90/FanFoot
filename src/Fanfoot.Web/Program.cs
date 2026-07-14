using Fanfoot.Domain;
using Fanfoot.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeysPath"] ?? "keys"));

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
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=fanfoot.db";
builder.Services.AddFanfootInfrastructure(connectionString);
builder.Services.AddFanfootDomain();

var groqApiKey = builder.Configuration["GroqApiKey"];
var deepSeekApiKey = builder.Configuration["DeepSeekApiKey"];
var ollamaUrl = builder.Configuration["OllamaUrl"] ?? "http://192.168.0.48:11434/";
builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress = new Uri(ollamaUrl);
    client.Timeout = TimeSpan.FromSeconds(120);
});

if (!string.IsNullOrEmpty(groqApiKey))
{
    builder.Services.AddHttpClient("Groq", client =>
    {
        client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", groqApiKey);
        client.Timeout = TimeSpan.FromSeconds(60);
    });
}

if (!string.IsNullOrEmpty(deepSeekApiKey))
{
    builder.Services.AddHttpClient("DeepSeek", client =>
    {
        client.BaseAddress = new Uri("https://api.deepseek.com/");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", deepSeekApiKey);
        client.Timeout = TimeSpan.FromSeconds(60);
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

var openApi = app.MapOpenApi();
var scalar = app.MapScalarApiReference("/scalar");

if (!app.Environment.IsDevelopment())
{
    openApi.RequireAuthorization();
    scalar.RequireAuthorization();
}

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
