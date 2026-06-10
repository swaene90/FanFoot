namespace Fanfoot.Domain.Models;

public class TradedPick
{
    public string Season { get; set; } = string.Empty;
    public int Round { get; set; }
    public int RosterId { get; set; }
    public int OwnerId { get; set; }
    public int PreviousOwnerId { get; set; }
}
