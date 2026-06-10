namespace Fanfoot.Infrastructure.Data.Entities;

public class LeagueEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = "sleeper";
    public string? Avatar { get; set; }
    public int Season { get; set; }
    public string? PreviousLeagueId { get; set; }
    public string? DraftId { get; set; }
    public int ScoringType { get; set; }
    public int RosterPositions { get; set; }
    public int TotalRosters { get; set; }
    public int? LastReadId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
