using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Fanfoot.Domain.Services;

public class UserService
{
    private readonly FanfootDbContext _db;

    public UserService(FanfootDbContext db) => _db = db;

    public async Task<User?> GetUserAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user == null ? null : EntityMapper.ToDomain(user);
    }

    public async Task<List<(int Season, List<(League League, List<Team> Teams)> Groups)>> GetUserTeamsBySeasonAsync(string userId)
    {
        var teams = (await _db.Teams.Where(t => t.OwnerId == userId).ToListAsync())
            .Select(EntityMapper.ToDomain).ToList();
        if (teams.Count == 0) return [];

        var leagueIds = teams.Select(t => t.LeagueId).Distinct().ToList();
        var leagues = (await _db.Leagues.Where(l => leagueIds.Contains(l.Id)).ToListAsync())
            .Select(EntityMapper.ToDomain).ToList();

        var leagueMap = leagues.ToDictionary(l => l.Id);

        return teams
            .GroupBy(t => leagueMap.TryGetValue(t.LeagueId, out var l) ? l.Season : 0)
            .OrderByDescending(g => g.Key)
            .Select(g => (
                Season: g.Key,
                Groups: g
                    .GroupBy(t => leagueMap.GetValueOrDefault(t.LeagueId)!)
                    .OrderBy(gg => gg.Key.Name)
                    .Select(gg => (League: gg.Key, Teams: gg.OrderBy(t => t.TeamName).ToList()))
                    .ToList()
            ))
            .ToList();
    }

    public async Task<List<League>> GetUserCurrentLeaguesAsync(string userId)
    {
        var teams = await _db.Teams.Where(t => t.OwnerId == userId).ToListAsync();
        var leagueIds = teams.Select(t => t.LeagueId).Distinct().ToList();
        var allLeagues = await _db.Leagues.Where(l => leagueIds.Contains(l.Id)).ToListAsync();
        if (allLeagues.Count == 0) return [];

        var currentSeason = allLeagues.Max(l => l.Season);
        return allLeagues
            .Where(l => l.Season == currentSeason)
            .OrderBy(l => l.Name)
            .Select(EntityMapper.ToDomain)
            .ToList();
    }
}
