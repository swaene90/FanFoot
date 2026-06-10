namespace Fanfoot.Infrastructure.Data.Entities;

public class MatchupEntity
{
    public string Id { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public int Week { get; set; }
    public int Season { get; set; }
    public string TeamId { get; set; } = string.Empty;
    public string? OpponentTeamId { get; set; }
    public double Points { get; set; }
    public double ProjectedPoints { get; set; }
    public double? OpponentPoints { get; set; }
    public string? Starters { get; set; }
    public string? StartersPoints { get; set; }
    public string? Players { get; set; }
    public string? PlayersPoints { get; set; }
    public int? MatchupId { get; set; }
    public bool? Won { get; set; }
}
