namespace Fanfoot.Infrastructure.Data.Entities;

public class LocalUserEntity
{
    public string SleeperUserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SessionVersion { get; set; } = 1;
}
