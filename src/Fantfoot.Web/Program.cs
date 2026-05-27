using Fantfoot.Infrastructure;
using Fantfoot.Infrastructure.Data;
using Fantfoot.Infrastructure.Services;
using Fantfoot.Web.Components;
using Fantfoot.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=fantfoot.db";
builder.Services.AddFantfootInfrastructure(connectionString);
builder.Services.AddHostedService<PlayerImportService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress = new Uri("http://192.168.0.48:11434/");
    client.Timeout = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FantfootDbContext>();
    db.Database.Migrate();
}
else
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
