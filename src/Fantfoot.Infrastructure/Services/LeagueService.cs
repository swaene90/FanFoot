using Microsoft.EntityFrameworkCore;
using Fantfoot.Domain;
using Fantfoot.Infrastructure.Data;
using Fantfoot.Infrastructure.Clients;
using Fantfoot.Infrastructure.Mapping;

namespace Fantfoot.Infrastructure.Services;

public class LeagueService
{
    private readonly FantfootDbContext _db;
    private readonly SleeperClient _sleeper;

    public LeagueService(FantfootDbContext db, SleeperClient sleeper)
    {
        _db = db;
        _sleeper = sleeper;
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
}
