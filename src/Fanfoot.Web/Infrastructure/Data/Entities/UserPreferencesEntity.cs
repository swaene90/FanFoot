namespace Fanfoot.Infrastructure.Data.Entities;

public class UserPreferencesEntity
{
    public string UserId { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; }
    public DateTime UpdatedAt { get; set; }
}
