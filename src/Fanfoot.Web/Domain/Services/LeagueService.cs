using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Clients;
using Fanfoot.Infrastructure.Mapping;

namespace Fanfoot.Domain.Services;

public class LeagueService
{
    private readonly FanfootDbContext _db;
    private readonly SleeperClient _sleeper;

    public LeagueService(FanfootDbContext db, SleeperClient sleeper)
    {
        _db = db;
        _sleeper = sleeper;
    }

    public async Task<List<League>> GetLeaguesAsync()
    {
        var leagues = await _db.Leagues.OrderByDescending(l => l.Season).ThenBy(l => l.Name).ToListAsync();
        return leagues.Select(EntityMapper.ToDomain).ToList();
    }

    public async Task<League?> GetLeagueAsync(string leagueId)
    {
        var league = await _db.Leagues.FindAsync(leagueId);
        return league == null ? null : EntityMapper.ToDomain(league);
    }

    public async Task<List<Team>> GetLeagueTeamsAsync(string leagueId)
    {
        var teams = await _db.Teams.Where(t => t.LeagueId == leagueId).ToListAsync();
        return teams.Select(EntityMapper.ToDomain).ToList();
    }

    public async Task<List<User>> GetLeagueUsersAsync(string leagueId)
    {
        var users = await _db.Users.Where(u => u.LeagueId == leagueId).ToListAsync();
        return users.Select(EntityMapper.ToDomain).ToList();
    }

    public async Task<TeamRoster?> GetTeamRosterAsync(string teamId)
    {
        var team = await _db.Teams.FindAsync(teamId);
        if (team == null) return null;

        var user = team.OwnerId != null
            ? await _db.Users.FirstOrDefaultAsync(u => u.Id == team.OwnerId)
            : null;

        var rosterIds = DeserializeIds(team.Roster);
        var starterIds = DeserializeIds(team.Starters);
        var reserveIds = DeserializeIds(team.Reserve);
        var taxiIds = DeserializeIds(team.Taxi);

        var allIds = rosterIds.Concat(reserveIds).Concat(taxiIds).Distinct().ToList();

        var players = allIds.Count > 0
            ? await _db.Players.Where(p => allIds.Contains(p.Id)).ToListAsync()
            : [];
        var playerMap = players.ToDictionary(p => p.Id, p => EntityMapper.ToDomain(p));

        List<Player> Resolve(IEnumerable<string> ids) =>
            ids.Select(id => playerMap.GetValueOrDefault(id)).OfType<Player>().ToList();

        return new TeamRoster
        {
            Team = EntityMapper.ToDomain(team),
            ManagerName = user?.DisplayName ?? "—",
            Starters = Resolve(starterIds),
            Bench = Resolve(rosterIds.Except(starterIds)),
            Reserve = Resolve(reserveIds),
            Taxi = Resolve(taxiIds)
        };
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
            await _db.SaveChangesAsync(ct);
            return EntityMapper.ToDomain(existing);
        }

        league.CreatedAt = DateTime.UtcNow;
        _db.Leagues.Add(league);
        await _db.SaveChangesAsync(ct);
        return EntityMapper.ToDomain(league);
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
        return teams.Select(EntityMapper.ToDomain).ToList();
    }

    public async Task<int> ImportPlayersAsync(CancellationToken ct = default)
    {
        var playerDtos = await _sleeper.GetAllPlayersAsync(ct) ?? [];
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
        var entities = users.Select(u => SleeperMapper.ToUser(u, leagueId)).ToList();

        foreach (var user in entities)
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
        return entities.Select(EntityMapper.ToDomain).ToList();
    }

    public async Task<List<DraftPick>> ImportDraftPicksAsync(string leagueId, CancellationToken ct = default)
    {
        var drafts = await _sleeper.GetLeagueDraftsAsync(leagueId, ct) ?? [];
        var draft = drafts.FirstOrDefault();
        if (draft == null) return [];

        var picks = await _sleeper.GetDraftPicksAsync(draft.DraftId, ct) ?? [];
        var entities = picks.Select(p => SleeperMapper.ToDraftPick(p, leagueId)).ToList();

        foreach (var pick in entities)
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
        return entities.Select(EntityMapper.ToDomain).ToList();
    }

    public async Task<DraftInfo?> GetDraftInfoAsync(string leagueId, CancellationToken ct = default)
    {
        var drafts = await _sleeper.GetLeagueDraftsAsync(leagueId, ct) ?? [];
        var draft = drafts.FirstOrDefault();
        return draft == null ? null : SleeperMapper.ToDraftInfo(draft);
    }

    public async Task<List<TradedPick>> GetTradedPicksAsync(string leagueId, CancellationToken ct = default)
    {
        var picks = await _sleeper.GetTradedPicksAsync(leagueId, ct) ?? [];
        return picks.Select(SleeperMapper.ToTradedPick).ToList();
    }

    private static List<string> DeserializeIds(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
