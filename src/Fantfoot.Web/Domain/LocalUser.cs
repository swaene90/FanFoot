namespace Fantfoot.Domain;

public class LocalUser
{
    public string SleeperUserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
