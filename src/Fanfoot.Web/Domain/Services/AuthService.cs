using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Clients;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Data.Entities;
using Fanfoot.Infrastructure.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace Fanfoot.Domain.Services;

public record RegisterResult(bool Success, string? Error, string? UserId);
public record PasswordResetResult(bool Success, string? Error);

public class AuthService
{
    private const string AuthorizedLeagueId = "1354116239798046720";

    private readonly FanfootDbContext _db;
    private readonly SleeperClient _sleeper;
    private readonly LeagueService _leagueService;
    private readonly IPasswordHasher<LocalUser> _hasher;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public AuthService(
        FanfootDbContext db,
        SleeperClient sleeper,
        LeagueService leagueService,
        IPasswordHasher<LocalUser> hasher,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _db = db;
        _sleeper = sleeper;
        _leagueService = leagueService;
        _hasher = hasher;
        _emailSender = emailSender;
        _configuration = configuration;
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

    public async Task<bool> IsSessionValidAsync(string userId, int sessionVersion)
    {
        return await _db.LocalUsers.AnyAsync(u =>
            u.SleeperUserId == userId && u.SessionVersion == sessionVersion);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _db.LocalUsers.FirstOrDefaultAsync(
            u => u.Email == normalizedEmail && u.PasswordHash != null,
            cancellationToken);
        if (user == null)
            return;

        var now = DateTime.UtcNow;
        var tokens = await _db.PasswordResetTokens
            .Where(t => t.SleeperUserId == user.SleeperUserId && t.UsedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var existing in tokens)
            existing.UsedAt = now;

        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        _db.PasswordResetTokens.Add(new PasswordResetTokenEntity
        {
            Id = Guid.NewGuid(),
            SleeperUserId = user.SleeperUserId,
            TokenHash = HashToken(token),
            CreatedAt = now,
            ExpiresAt = now.AddHours(1)
        });
        await _db.SaveChangesAsync(cancellationToken);

        var publicUrl = _configuration["App:PublicUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(publicUrl) || !Uri.TryCreate(publicUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException("App:PublicUrl must be configured as an absolute URL.");

        var resetUrl = $"{publicUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        await _emailSender.SendPasswordResetAsync(user.Email!, resetUrl, cancellationToken);
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(string token, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return new PasswordResetResult(false, "The reset link or password is invalid.");

        var now = DateTime.UtcNow;
        var tokenHash = HashToken(token);
        var resetToken = await _db.PasswordResetTokens.SingleOrDefaultAsync(
            t => t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > now,
            cancellationToken);
        if (resetToken == null)
            return new PasswordResetResult(false, "The reset link is invalid or has expired.");

        var user = await _db.LocalUsers.FindAsync([resetToken.SleeperUserId], cancellationToken);
        if (user == null)
            return new PasswordResetResult(false, "The reset link is invalid or has expired.");

        var domainUser = EntityMapper.ToDomain(user);
        user.PasswordHash = _hasher.HashPassword(domainUser, password);
        user.SessionVersion++;
        resetToken.UsedAt = now;
        await _db.SaveChangesAsync(cancellationToken);
        return new PasswordResetResult(true, null);
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

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
