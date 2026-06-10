namespace Fanfoot.Domain.Models;

public class Team
{
    public string Id { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? OwnerId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public double PointsFor { get; set; }
    public double PointsAgainst { get; set; }
    public int? Standing { get; set; }
    public int? WaiverPriority { get; set; }
    public int? FaabBudget { get; set; }
    public string? Settings { get; set; }
    public string? Roster { get; set; }
    public string? Starters { get; set; }
    public string? Taxi { get; set; }
    public string? Practice { get; set; }
    public string? Reserve { get; set; }
    public int? WaiverPosition { get; set; }
    public int? TradeBank { get; set; }
}
