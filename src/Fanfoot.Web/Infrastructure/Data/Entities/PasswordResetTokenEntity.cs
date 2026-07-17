namespace Fanfoot.Infrastructure.Data.Entities;

public class PasswordResetTokenEntity
{
    public Guid Id { get; set; }
    public string SleeperUserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
