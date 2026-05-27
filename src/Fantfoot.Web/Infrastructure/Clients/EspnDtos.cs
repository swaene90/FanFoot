using System.Text.Json.Serialization;

namespace Fantfoot.Infrastructure.Clients;

public class EspnNewsResponseDto
{
    [JsonPropertyName("articles")]
    public List<EspnArticleDto>? Articles { get; set; }
}

public class EspnArticleDto
{
    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("published")]
    public string? Published { get; set; }

    [JsonPropertyName("athletes")]
    public List<EspnAthleteDto>? Athletes { get; set; }
}

public class EspnAthleteDto
{
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}
