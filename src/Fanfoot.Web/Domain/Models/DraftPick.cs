namespace Fanfoot.Domain.Models;

public class DraftPick
{
    public string Id { get; set; } = string.Empty;
    public string DraftId { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public int Round { get; set; }
    public int PickNumber { get; set; }
    public int? OverallPickNumber { get; set; }
    public string? TeamId { get; set; }
    public string? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public string? Position { get; set; }
    public string? Team { get; set; }
    public bool? IsKeeper { get; set; }
    public DateTime CreatedAt { get; set; }
}
