using Fanfoot.Domain.Models;
using Fanfoot.Domain.Services;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Fanfoot.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RequestPasswordReset_CreatesHashedToken_AndSupersedesPreviousToken()
    {
        await using var context = CreateContext();
        var sender = new FakeEmailSender();
        var service = CreateService(context, sender);
        await AddUserAsync(context);

        await service.RequestPasswordResetAsync("user@example.com");
        var first = await context.PasswordResetTokens.SingleAsync();
        Assert.NotNull(sender.ResetUrl);
        Assert.DoesNotContain(GetToken(sender.ResetUrl!), first.TokenHash);

        await service.RequestPasswordResetAsync("user@example.com");
        Assert.NotNull(first.UsedAt);
        Assert.Equal(2, await context.PasswordResetTokens.CountAsync());
    }

    [Fact]
    public async Task ResetPassword_ConsumesToken_ChangesPassword_AndInvalidatesSessions()
    {
        await using var context = CreateContext();
        var sender = new FakeEmailSender();
        var service = CreateService(context, sender);
        await AddUserAsync(context);
        await service.RequestPasswordResetAsync("user@example.com");

        var result = await service.ResetPasswordAsync(GetToken(sender.ResetUrl!), "new-password");

        Assert.True(result.Success);
        Assert.Null(await service.ValidateCredentialsAsync("user@example.com", "old-password"));
        Assert.NotNull(await service.ValidateCredentialsAsync("user@example.com", "new-password"));
        Assert.False(await service.IsSessionValidAsync("user-1", 1));
        Assert.True(await service.IsSessionValidAsync("user-1", 2));
        Assert.False((await service.ResetPasswordAsync(GetToken(sender.ResetUrl!), "another-password")).Success);
    }

    [Fact]
    public async Task RequestPasswordReset_UnknownEmail_DoesNotSendEmail()
    {
        await using var context = CreateContext();
        var sender = new FakeEmailSender();
        var service = CreateService(context, sender);

        await service.RequestPasswordResetAsync("unknown@example.com");

        Assert.Null(sender.ResetUrl);
        Assert.Empty(context.PasswordResetTokens);
    }

    [Fact]
    public async Task ResetPassword_RejectsExpiredToken()
    {
        await using var context = CreateContext();
        var sender = new FakeEmailSender();
        var service = CreateService(context, sender);
        await AddUserAsync(context);
        await service.RequestPasswordResetAsync("user@example.com");
        context.PasswordResetTokens.Single().ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await context.SaveChangesAsync();

        var result = await service.ResetPasswordAsync(GetToken(sender.ResetUrl!), "new-password");

        Assert.False(result.Success);
        Assert.NotNull(await service.ValidateCredentialsAsync("user@example.com", "old-password"));
    }

    private static FanfootDbContext CreateContext() => new(
        new DbContextOptionsBuilder<FanfootDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AuthService CreateService(FanfootDbContext context, IEmailSender sender) => new(
        context, null!, null!, new PasswordHasher<LocalUser>(), sender,
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["App:PublicUrl"] = "https://fanfoot.swaenepoel.org"
        }).Build());

    private static async Task AddUserAsync(FanfootDbContext context)
    {
        var hasher = new PasswordHasher<LocalUser>();
        context.LocalUsers.Add(new LocalUserEntity
        {
            SleeperUserId = "user-1", Email = "user@example.com", CreatedAt = DateTime.UtcNow,
            PasswordHash = hasher.HashPassword(new LocalUser(), "old-password"), SessionVersion = 1
        });
        await context.SaveChangesAsync();
    }

    private static string GetToken(string url) => Uri.UnescapeDataString(url.Split("token=")[1]);

    private sealed class FakeEmailSender : IEmailSender
    {
        public string? ResetUrl { get; private set; }
        public Task SendPasswordResetAsync(string email, string resetUrl, CancellationToken cancellationToken = default)
        {
            ResetUrl = resetUrl;
            return Task.CompletedTask;
        }
    }
}
