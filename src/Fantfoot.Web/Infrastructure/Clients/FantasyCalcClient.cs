using System.Net.Http.Json;
using System.Text.Json;

namespace Fantfoot.Infrastructure.Clients;

public class FantasyCalcClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public FantasyCalcClient(HttpClient http) => _http = http;

    public async Task<List<FantasyCalcValueDto>?> GetValuesAsync(
        bool isDynasty, int numQbs, bool isPpr, int numTeams,
        CancellationToken ct = default)
    {
        var url = $"values/current?isDynasty={isDynasty.ToString().ToLower()}&numQbs={numQbs}&numTeams={numTeams}&ppr={( isPpr ? "1" : "0" )}";
        return await _http.GetFromJsonAsync<List<FantasyCalcValueDto>>(url, JsonOptions, ct);
    }
}
