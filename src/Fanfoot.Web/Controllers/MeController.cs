using System.Security.Claims;
using Fanfoot.Domain.Models;
using Fanfoot.Domain.Services;
using Fanfoot.Web.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fanfoot.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public class MeController(UserService users, PreferencesService preferences) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeDto>> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await users.GetUserAsync(userId);
        var seasons = await users.GetUserTeamsBySeasonAsync(userId);
        var currentLeagues = await users.GetUserCurrentLeaguesAsync(userId);
        return Ok(new MeDto(ToUser(userId, user, User.FindFirstValue(ClaimTypes.Email)), currentLeagues.Select(ToLeague).ToList(),
            seasons.Select(s => new SeasonTeamsDto(s.Season, s.Groups.Select(g => new LeagueTeamsDto(ToLeague(g.League), g.Teams.Select(team => ToTeam(team)).ToList())).ToList())).ToList()));
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<PreferencesDto>> GetPreferences()
    {
        var prefs = await preferences.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new PreferencesDto(prefs?.IsDarkMode ?? false));
    }

    [HttpPut("preferences")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<PreferencesDto>> UpdatePreferences(UpdatePreferencesRequest request)
    {
        await preferences.SetDarkModeAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request.IsDarkMode);
        return Ok(new PreferencesDto(request.IsDarkMode));
    }

    internal static AuthUserDto ToUser(string id, User? user, string? email) => new(id, user?.DisplayName, email ?? user?.Email);
    internal static LeagueDto ToLeague(League league) => new(league.Id, league.Name, league.Source, league.Avatar, league.Season, league.PreviousLeagueId, league.TotalRosters);
    internal static TeamDto ToTeam(Team team, string? managerName = null) => new(team.Id, team.LeagueId, team.OwnerId, team.TeamName, team.Wins, team.Losses, team.Ties, team.PointsFor, team.PointsAgainst, managerName);
    internal static PlayerDto ToPlayer(Player player) => new(player.Id, player.FullName, player.Position, player.Team, player.Status, player.InjuryStatus);
}
