namespace Fanfoot.Domain.Models;

public class DraftInfo
{
    public string DraftId { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Season { get; set; }
    public Dictionary<string, int>? DraftOrder { get; set; }
    public Dictionary<string, int>? SlotToRosterId { get; set; }
    public int? Rounds { get; set; }
    public int? Teams { get; set; }
}
