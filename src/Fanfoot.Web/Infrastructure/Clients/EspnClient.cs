using System.Net.Http.Json;
using System.Text.Json;

namespace Fanfoot.Infrastructure.Clients;

public class EspnClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public EspnClient(HttpClient http) => _http = http;

    public async Task<EspnNewsResponseDto?> GetNewsAsync(int limit = 100, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<EspnNewsResponseDto>(
            $"apis/site/v2/sports/football/nfl/news?limit={limit}", JsonOptions, ct);
    }
}
