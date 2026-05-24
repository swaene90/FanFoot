using System.Net.Http.Json;
using System.Text.Json;
using Fantfoot.Domain;

namespace Fantfoot.Infrastructure.Clients;

public class SleeperClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string BaseUrl = "https://api.sleeper.app/v1/";

    public SleeperClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<Dictionary<string, SleeperPlayerDto>?> GetAllPlayersAsync(CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<Dictionary<string, SleeperPlayerDto>>("players/nfl", JsonOptions, ct);
    }

    public async Task<SleeperLeagueDto?> GetLeagueAsync(string leagueId, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<SleeperLeagueDto>($"league/{leagueId}", JsonOptions, ct);
    }

    public async Task<List<SleeperRosterDto>?> GetLeagueRostersAsync(string leagueId, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<SleeperRosterDto>>($"league/{leagueId}/rosters", JsonOptions, ct);
    }

    public async Task<List<SleeperUserDto>?> GetLeagueUsersAsync(string leagueId, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<SleeperUserDto>>($"league/{leagueId}/users", JsonOptions, ct);
    }

    public async Task<List<SleeperMatchupDto>?> GetLeagueMatchupsAsync(string leagueId, int week, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<SleeperMatchupDto>>($"league/{leagueId}/matchups/{week}", JsonOptions, ct);
    }

    public async Task<List<SleeperDraftPickDto>?> GetDraftPicksAsync(string draftId, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<SleeperDraftPickDto>>($"draft/{draftId}/picks", JsonOptions, ct);
    }

    public async Task<SleeperNflStateDto?> GetNflStateAsync(CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<SleeperNflStateDto>("state/nfl", JsonOptions, ct);
    }

    public async Task<List<SleeperLeagueDto>?> GetUserLeaguesAsync(string userId, string season, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<SleeperLeagueDto>>($"user/{userId}/leagues/nfl/{season}", JsonOptions, ct);
    }

    public async Task<List<SleeperDraftPickDto>?> GetLeagueDraftsAsync(string leagueId, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<SleeperDraftPickDto>>($"league/{leagueId}/drafts", JsonOptions, ct);
    }
}
