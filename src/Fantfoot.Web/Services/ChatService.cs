using System.Text.Json;
using System.Text.Json.Serialization;
using Fantfoot.Domain;
using Fantfoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fantfoot.Web.Services;

public class ChatService
{
    private readonly FantfootDbContext _db;
    private readonly HttpClient _http;
    private const string Model = "llama3.1:8b";

    public ChatService(FantfootDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _http = httpClientFactory.CreateClient("Ollama");
    }

    public async Task<string> GetUserContextAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return "";

        var teams = await _db.Teams
            .Where(t => t.OwnerId == userId)
            .ToListAsync();

        var leagueIds = teams.Select(t => t.LeagueId).Distinct().ToList();
        var leagues = await _db.Leagues
            .Where(l => leagueIds.Contains(l.Id))
            .ToListAsync();
        var leagueMap = leagues.ToDictionary(l => l.Id);

        var playerIds = teams
            .Where(t => !string.IsNullOrEmpty(t.Roster))
            .SelectMany(t => JsonSerializer.Deserialize<List<string>>(t.Roster!) ?? [])
            .Distinct()
            .ToList();

        var players = playerIds.Count > 0
            ? await _db.Players.Where(p => playerIds.Contains(p.Id)).ToListAsync()
            : [];
        var playerMap = players.ToDictionary(p => p.Id);

        var context = $"You are an AI assistant for a fantasy football tracker app. ";
        context += $"The current user is {user.DisplayName}.";
        context += $"\n\nThey have {teams.Count} team(s) across {leagues.Count} league(s):\n";

        foreach (var team in teams)
        {
            var league = leagueMap.GetValueOrDefault(team.LeagueId);
            var leagueName = league?.Name ?? "Unknown League";
            var season = league?.Season.ToString() ?? "?";

            context += $"\n--- {team.TeamName} ({leagueName}, {season}) ---";
            context += $"\nRecord: {team.Wins}-{team.Losses}-{team.Ties}";
            context += $"\nPoints For: {team.PointsFor:F1}, Points Against: {team.PointsAgainst:F1}";

            var starterIds = JsonSerializer.Deserialize<List<string>>(team.Starters ?? "[]") ?? [];
            var rosterIds = JsonSerializer.Deserialize<List<string>>(team.Roster ?? "[]") ?? [];
            var benchIds = rosterIds.Except(starterIds).ToList();

            if (starterIds.Count > 0)
            {
                var starterNames = starterIds
                    .Select(id => playerMap.GetValueOrDefault(id))
                    .OfType<Player>()
                    .Select(p => $"{p.FirstName} {p.LastName} ({p.Position})");
                context += $"\nStarters: {string.Join(", ", starterNames)}";
            }

            if (benchIds.Count > 0)
            {
                var benchNames = benchIds
                    .Select(id => playerMap.GetValueOrDefault(id))
                    .OfType<Player>()
                    .Select(p => $"{p.FirstName} {p.LastName} ({p.Position})");
                context += $"\nBench: {string.Join(", ", benchNames)}";
            }
        }

        return context;
    }

    public async Task<string> GetLeagueContextAsync(string leagueId)
    {
        var league = await _db.Leagues.FindAsync(leagueId);
        if (league == null) return "";

        var teams = await _db.Teams.Where(t => t.LeagueId == leagueId).OrderByDescending(t => t.Wins).ToListAsync();
        var users = await _db.Users.Where(u => u.LeagueId == leagueId).ToListAsync();
        var userMap = users.ToDictionary(u => u.Id, u => u.DisplayName ?? u.Id);

        var context = $"You are an AI assistant for a fantasy football tracker app. ";
        context += $"The current league is {league.Name} ({league.Season} season).";
        context += $"\nIt has {league.TotalRosters} teams.";
        context += $"\n\nStandings:\n";

        foreach (var team in teams)
        {
            var manager = team.OwnerId != null && userMap.TryGetValue(team.OwnerId, out var name) ? name : "Unknown";
            context += $"\n{team.TeamName} (managed by {manager}): {team.Wins}-{team.Losses}-{team.Ties}, PF: {team.PointsFor:F1}, PA: {team.PointsAgainst:F1}";
        }

        return context;
    }

    public async Task<string> GetTeamContextAsync(string teamId)
    {
        var team = await _db.Teams.FindAsync(teamId);
        if (team == null) return "";

        var league = await _db.Leagues.FindAsync(team.LeagueId);
        var user = team.OwnerId != null ? await _db.Users.FindAsync(team.OwnerId) : null;

        var starterIds = JsonSerializer.Deserialize<List<string>>(team.Starters ?? "[]") ?? [];
        var rosterIds = JsonSerializer.Deserialize<List<string>>(team.Roster ?? "[]") ?? [];
        var reserveIds = JsonSerializer.Deserialize<List<string>>(team.Reserve ?? "[]") ?? [];
        var taxiIds = JsonSerializer.Deserialize<List<string>>(team.Taxi ?? "[]") ?? [];
        var allIds = rosterIds.Concat(reserveIds).Concat(taxiIds).Distinct().ToList();

        var players = allIds.Count > 0
            ? await _db.Players.Where(p => allIds.Contains(p.Id)).ToListAsync()
            : [];
        var playerMap = players.ToDictionary(p => p.Id);

        var managerName = user?.DisplayName ?? "Unknown";

        var context = $"You are an AI assistant for a fantasy football tracker app. ";
        context += $"The current team is {team.TeamName} (managed by {managerName}).";
        context += $"\nLeague: {league?.Name ?? "Unknown"}";
        context += $"\nRecord: {team.Wins}-{team.Losses}-{team.Ties}";
        context += $"\nPoints For: {team.PointsFor:F1}, Points Against: {team.PointsAgainst:F1}";

        if (starterIds.Count > 0)
        {
            var starterNames = starterIds
                .Select(id => playerMap.GetValueOrDefault(id))
                .OfType<Player>()
                .Select(p => $"{p.FirstName} {p.LastName} ({p.Position})");
            context += $"\nStarters: {string.Join(", ", starterNames)}";
        }

        var benchIds = rosterIds.Except(starterIds).ToList();
        if (benchIds.Count > 0)
        {
            var benchNames = benchIds
                .Select(id => playerMap.GetValueOrDefault(id))
                .OfType<Player>()
                .Select(p => $"{p.FirstName} {p.LastName} ({p.Position})");
            context += $"\nBench: {string.Join(", ", benchNames)}";
        }

        if (reserveIds.Count > 0)
        {
            var reserveNames = reserveIds
                .Select(id => playerMap.GetValueOrDefault(id))
                .OfType<Player>()
                .Select(p => $"{p.FirstName} {p.LastName} ({p.Position})");
            context += $"\nReserve/IR: {string.Join(", ", reserveNames)}";
        }

        return context;
    }

    public async Task<string> AskAsync(string systemPrompt, string question)
    {
        var request = new OllamaChatRequest
        {
            Model = Model,
            Messages =
            [
                new OllamaMessage { Role = "system", Content = systemPrompt },
                new OllamaMessage { Role = "user", Content = question }
            ],
            Stream = false
        };

        var response = await _http.PostAsJsonAsync("api/chat", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();
        return result?.Message?.Content ?? "Sorry, I couldn't process that.";
    }

    private class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public OllamaMessage[] Messages { get; set; } = [];

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }
    }
}
