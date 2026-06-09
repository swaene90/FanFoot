using System.Text.Json;
using Fanfoot.Domain;
using Fanfoot.Infrastructure.Clients;
using Fanfoot.Infrastructure.Mapping;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fanfoot.Infrastructure.Data;

public static class DatabaseSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<FanfootDbContext>();

        if (await db.LocalUsers.AnyAsync())
            return;

        var hasher = services.GetRequiredService<IPasswordHasher<LocalUser>>();
        var env = services.GetRequiredService<IWebHostEnvironment>();

        var user = new LocalUser
        {
            SleeperUserId = "dev_user",
            Email = "admin@local.dev",
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, "password");
        db.LocalUsers.Add(user);

        db.Leagues.Add(new League
        {
            Id = "dev_league",
            Name = "Dev League",
            Source = "local",
            Season = 2024,
            TotalRosters = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        await SeedPlayersAsync(db, env);
    }

    private static async Task SeedPlayersAsync(FanfootDbContext db, IWebHostEnvironment env)
    {
        var filePath = Path.Combine(env.ContentRootPath, "Data", "players.json");
        await using var stream = File.OpenRead(filePath);
        var playerDtos = await JsonSerializer.DeserializeAsync<Dictionary<string, SleeperPlayerDto>>(stream, JsonOptions) ?? [];

        var players = playerDtos.Select(kvp => SleeperMapper.ToPlayer(kvp.Value)).ToList();

        var existingIds = await db.Players.Select(p => p.Id).ToListAsync();
        var existingSet = new HashSet<string>(existingIds);

        db.Players.AddRange(players.Where(p => !existingSet.Contains(p.Id)));
        await db.SaveChangesAsync();
    }
}
