using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Clients;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Data.Entities;
using Fanfoot.Infrastructure.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fanfoot.Domain.Services;

public record RegisterResult(bool Success, string? Error, string? UserId);

public class AuthService
{
    private const string AuthorizedLeagueId = "1354116239798046720";

    private readonly FanfootDbContext _db;
    private readonly SleeperClient _sleeper;
    private readonly LeagueService _leagueService;
    private readonly IPasswordHasher<LocalUser> _hasher;

    public AuthService(
        FanfootDbContext db,
        SleeperClient sleeper,
        LeagueService leagueService,
        IPasswordHasher<LocalUser> hasher)
    {
        _db = db;
        _sleeper = sleeper;
        _leagueService = leagueService;
        _hasher = hasher;
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        return await _db.LocalUsers.FindAsync(userId) != null;
    }

    public async Task<LocalUser?> ValidateCredentialsAsync(string email, string password)
    {
        var normalized = email.Trim().ToLower();
        var user = await _db.LocalUsers.FirstOrDefaultAsync(u => u.Email == normalized);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        var domainUser = EntityMapper.ToDomain(user);
        var result = _hasher.VerifyHashedPassword(domainUser, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : domainUser;
    }

    public async Task<RegisterResult> RegisterAsync(
        string email, string password, string sleeperUsername,
        Action<string>? onProgress = null)
    {
        var emailNorm = email.Trim().ToLower();

        var emailTaken = await _db.LocalUsers.AnyAsync(u => u.Email == emailNorm);
        if (emailTaken)
            return new RegisterResult(false, "An account with this email already exists.", null);

        onProgress?.Invoke("Looking up your Sleeper account...");
        var sleeperUser = await _sleeper.GetUserAsync(sleeperUsername.Trim());
        if (sleeperUser == null || string.IsNullOrEmpty(sleeperUser.UserId))
            return new RegisterResult(false, "Sleeper username not found.", null);

        var userId = sleeperUser.UserId;

        onProgress?.Invoke("Checking your leagues...");
        var nflState = await _sleeper.GetNflStateAsync()
            ?? throw new InvalidOperationException("Could not reach the Sleeper API.");

        var authorized = false;
        foreach (var season in new[] { nflState.Season, (int.Parse(nflState.Season) - 1).ToString() })
        {
            var leagues = await _sleeper.GetUserLeaguesAsync(userId, season);
            if (leagues != null && leagues.Any(l => l.LeagueId == AuthorizedLeagueId))
            {
                authorized = true;
                break;
            }
        }

        if (!authorized)
            return new RegisterResult(false, "You are not a member of the authorized league.", null);

        var hash = _hasher.HashPassword(new LocalUser(), password);
        var existing = await _db.LocalUsers.FindAsync(userId);

        if (existing == null)
        {
            onProgress?.Invoke("Importing your leagues...");
            await ImportUserLeaguesAsync(userId, nflState.Season, onProgress);

            _db.LocalUsers.Add(new LocalUserEntity
            {
                SleeperUserId = userId,
                Email = emailNorm,
                PasswordHash = hash,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            // Legacy user — link email/password to existing record
            existing.Email = emailNorm;
            existing.PasswordHash = hash;
        }

        await _db.SaveChangesAsync();
        return new RegisterResult(true, null, userId);
    }

    private async Task ImportUserLeaguesAsync(string userId, string currentSeason, Action<string>? onProgress)
    {
        var seen = new HashSet<string>();
        foreach (var season in new[] { currentSeason, (int.Parse(currentSeason) - 1).ToString() })
        {
            var leagues = await _sleeper.GetUserLeaguesAsync(userId, season);
            if (leagues == null) continue;
            foreach (var league in leagues)
            {
                if (!seen.Add(league.LeagueId)) continue;
                onProgress?.Invoke($"Importing {league.Name}...");
                await _leagueService.ImportLeagueAsync(league.LeagueId);
                await _leagueService.ImportUsersAsync(league.LeagueId);
                await _leagueService.ImportRostersAsync(league.LeagueId);
            }
        }
    }
}
