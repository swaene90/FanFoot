namespace Fanfoot.Domain;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string? LeagueId { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? RealName { get; set; }
    public string? Email { get; set; }
    public bool IsOwner { get; set; }
    public bool IsCommissioner { get; set; }
    public int? UserMessage { get; set; }
    public string? Metadata { get; set; }
}
