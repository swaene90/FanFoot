namespace Fanfoot.Infrastructure.Data.Entities;

public class ChatSessionEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string? LeagueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MessagesJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
