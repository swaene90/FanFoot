using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Fanfoot.Domain;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Clients;
using Fanfoot.Infrastructure.Mapping;

namespace Fanfoot.Infrastructure.Services;

public class LeagueService
{
    private readonly FanfootDbContext _db;
    private readonly SleeperClient _sleeper;
    private readonly IWebHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LeagueService(FanfootDbContext db, SleeperClient sleeper, IWebHostEnvironment env)
    {
        _db = db;
        _sleeper = sleeper;
        _env = env;
    }

    public async Task<List<League>> GetLeaguesAsync()
    {
        return await _db.Leagues.OrderByDescending(l => l.Season).ThenBy(l => l.Name).ToListAsync();
    }

    public async Task<League?> GetLeagueAsync(string leagueId)
    {
        return await _db.Leagues.FindAsync(leagueId);
    }

    public async Task<League> ImportLeagueAsync(string leagueId, CancellationToken ct = default)
    {
        var dto = await _sleeper.GetLeagueAsync(leagueId, ct)
            ?? throw new InvalidOperationException($"League {leagueId} not found");

        var league = SleeperMapper.ToLeague(dto);
        league.UpdatedAt = DateTime.UtcNow;

        var existing = await _db.Leagues.FindAsync(new object[] { leagueId }, ct);
        if (existing != null)
        {
            existing.Name = league.Name;
            existing.Avatar = league.Avatar;
            existing.Season = league.Season;
            existing.PreviousLeagueId = league.PreviousLeagueId;
            existing.TotalRosters = league.TotalRosters;
            existing.Metadata = league.Metadata;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            league.CreatedAt = DateTime.UtcNow;
            _db.Leagues.Add(league);
        }

        await _db.SaveChangesAsync(ct);
        return league;
    }

    public async Task<List<Team>> ImportRostersAsync(string leagueId, CancellationToken ct = default)
    {
        var rosters = await _sleeper.GetLeagueRostersAsync(leagueId, ct) ?? [];
        var users = await _sleeper.GetLeagueUsersAsync(leagueId, ct) ?? [];
        var userTeamNames = users
            .Where(u => u.Metadata != null && u.Metadata.ContainsKey("team_name"))
            .ToDictionary(u => u.UserId, u => u.Metadata!["team_name"]);
        var teams = rosters.Select(r => SleeperMapper.ToTeam(r, leagueId, userTeamNames)).ToList();

        foreach (var team in teams)
        {
            var existing = await _db.Teams.FindAsync(new object[] { team.Id }, ct);
            if (existing != null)
            {
                existing.OwnerId = team.OwnerId;
                existing.TeamName = team.TeamName;
                existing.Wins = team.Wins;
                existing.Losses = team.Losses;
                existing.Ties = team.Ties;
                existing.PointsFor = team.PointsFor;
                existing.PointsAgainst = team.PointsAgainst;
                existing.Roster = team.Roster;
                existing.Starters = team.Starters;
                existing.Reserve = team.Reserve;
                existing.Taxi = team.Taxi;
            }
            else
            {
                _db.Teams.Add(team);
            }
        }

        await _db.SaveChangesAsync(ct);
        return teams;
    }

    public async Task<int> ImportPlayersAsync(CancellationToken ct = default)
    {
        Dictionary<string, SleeperPlayerDto> playerDtos;
        var filePath = Path.Combine(_env.ContentRootPath, "Data", "players.json");
        if (_env.IsDevelopment() && File.Exists(filePath))
        {
            await using var stream = File.OpenRead(filePath);
            playerDtos = await JsonSerializer.DeserializeAsync<Dictionary<string, SleeperPlayerDto>>(stream, JsonOptions, ct) ?? [];
        }
        else
        {
            playerDtos = await _sleeper.GetAllPlayersAsync(ct) ?? [];
        }
        var players = playerDtos.Select(kvp => SleeperMapper.ToPlayer(kvp.Value)).ToList();

        var existingIds = await _db.Players.Select(p => p.Id).ToListAsync(ct);
        var existingSet = new HashSet<string>(existingIds);

        var newPlayers = players.Where(p => !existingSet.Contains(p.Id)).ToList();
        _db.Players.AddRange(newPlayers);

        foreach (var player in players.Where(p => existingSet.Contains(p.Id)))
        {
            var existing = await _db.Players.FindAsync([player.Id], ct);
            if (existing != null)
            {
                existing.FirstName = player.FirstName;
                existing.LastName = player.LastName;
                existing.Position = player.Position;
                existing.Team = player.Team;
                existing.Number = player.Number;
                existing.Age = player.Age;
                existing.Height = player.Height;
                existing.Weight = player.Weight;
                existing.College = player.College;
                existing.Status = player.Status;
                existing.InjuryStatus = player.InjuryStatus;
                existing.FantasyPositions = player.FantasyPositions;
                existing.YearsExp = player.YearsExp;
                existing.Metadata = player.Metadata;
            }
        }

        await _db.SaveChangesAsync(ct);
        return players.Count;
    }

    public async Task<List<User>> ImportUsersAsync(string leagueId, CancellationToken ct = default)
    {
        var users = await _sleeper.GetLeagueUsersAsync(leagueId, ct) ?? [];
        var domainUsers = users.Select(u => SleeperMapper.ToUser(u, leagueId)).ToList();

        foreach (var user in domainUsers)
        {
            var existing = await _db.Users.FindAsync(new object[] { user.Id }, ct);
            if (existing != null)
            {
                existing.DisplayName = user.DisplayName;
                existing.Avatar = user.Avatar;
                existing.IsOwner = user.IsOwner;
                existing.IsCommissioner = user.IsCommissioner;
            }
            else
            {
                _db.Users.Add(user);
            }
        }

        await _db.SaveChangesAsync(ct);
        return domainUsers;
    }

    public async Task<List<DraftPick>> ImportDraftPicksAsync(string leagueId, CancellationToken ct = default)
    {
        var drafts = await _sleeper.GetLeagueDraftsAsync(leagueId, ct) ?? [];
        var draft = drafts.FirstOrDefault();
        if (draft == null) return [];

        var picks = await _sleeper.GetDraftPicksAsync(draft.DraftId, ct) ?? [];
        var domainPicks = picks.Select(p => SleeperMapper.ToDraftPick(p, leagueId)).ToList();

        foreach (var pick in domainPicks)
        {
            var existing = await _db.DraftPicks.FindAsync([pick.Id], ct);
            if (existing != null)
            {
                existing.PlayerName = pick.PlayerName;
                existing.Position = pick.Position;
                existing.Team = pick.Team;
                existing.IsKeeper = pick.IsKeeper;
            }
            else
            {
                _db.DraftPicks.Add(pick);
            }
        }

        await _db.SaveChangesAsync(ct);
        return domainPicks;
    }

    public async Task<SleeperDraftDto?> GetDraftInfoAsync(string leagueId, CancellationToken ct = default)
    {
        var drafts = await _sleeper.GetLeagueDraftsAsync(leagueId, ct) ?? [];
        return drafts.FirstOrDefault();
    }

    public async Task<List<SleeperTradedPickDto>> GetTradedPicksAsync(string leagueId, CancellationToken ct = default)
    {
        return await _sleeper.GetTradedPicksAsync(leagueId, ct) ?? [];
    }
}
