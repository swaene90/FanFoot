namespace Fanfoot.Infrastructure.Data.Entities;

public class PlayerStatsEntity
{
    public string Id { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public int Season { get; set; }
    public int Week { get; set; }
    public double Points { get; set; }
    public double ProjectedPoints { get; set; }
    public string? Stats { get; set; }
    public DateTime CreatedAt { get; set; }
}
