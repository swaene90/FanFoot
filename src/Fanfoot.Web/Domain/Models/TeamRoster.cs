namespace Fanfoot.Domain.Models;

public class TeamRoster
{
    public required Team Team { get; init; }
    public string ManagerName { get; init; } = "—";
    public List<Player> Starters { get; init; } = [];
    public List<Player> Bench { get; init; } = [];
    public List<Player> Reserve { get; init; } = [];
    public List<Player> Taxi { get; init; } = [];
}
