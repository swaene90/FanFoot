namespace Fanfoot.Domain;

public class UserPreferences
{
    public string UserId { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; }
    public DateTime UpdatedAt { get; set; }
}
