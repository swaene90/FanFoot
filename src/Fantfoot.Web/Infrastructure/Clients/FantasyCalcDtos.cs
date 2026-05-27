using System.Text.Json.Serialization;

namespace Fantfoot.Infrastructure.Clients;

public class FantasyCalcValueDto
{
    [JsonPropertyName("player")]
    public FantasyCalcPlayerInfoDto? Player { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("overallRank")]
    public int OverallRank { get; set; }

    [JsonPropertyName("positionRank")]
    public int PositionRank { get; set; }

    [JsonPropertyName("trend30Day")]
    public int? Trend30Day { get; set; }

    [JsonPropertyName("redraftValue")]
    public int? RedraftValue { get; set; }
}

public class FantasyCalcPlayerInfoDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("maybeTeam")]
    public string? MaybeTeam { get; set; }

    [JsonPropertyName("sleeperId")]
    public string? SleeperId { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }
}
