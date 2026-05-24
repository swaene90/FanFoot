namespace Fantfoot.Domain;

public class Player
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Position { get; set; }
    public string? Team { get; set; }
    public int? Number { get; set; }
    public int? Age { get; set; }
    public int? Height { get; set; }
    public int? Weight { get; set; }
    public string? College { get; set; }
    public string? Status { get; set; }
    public string? InjuryStatus { get; set; }
    public string? FantasyPositions { get; set; }
    public int? YearsExp { get; set; }
    public string? SearchFirstName { get; set; }
    public string? SearchLastName { get; set; }
    public string? SearchFullName { get; set; }
    public string? Metadata { get; set; }
}
