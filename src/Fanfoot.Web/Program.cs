using Fanfoot.Infrastructure;
using MudBlazor.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Fanfoot.Domain;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Services;
using Fanfoot.Web.Components;
using Fanfoot.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeysPath"] ?? "keys"));

builder.Services.AddMudServices();
builder.Services.AddScoped<IPasswordHasher<LocalUser>, PasswordHasher<LocalUser>>();

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
    var db = scope.ServiceProvider.GetRequiredService<FanfootDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapPost("/api/players/import", async (LeagueService leagueService) =>
{
    var count = await leagueService.ImportPlayersAsync();
    return Results.Ok(new { imported = count });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
