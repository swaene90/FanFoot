using System.Security.Claims;
using Fanfoot.Domain.Services;
using Fanfoot.Web.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fanfoot.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/leagues")]
public class LeaguesController(LeagueService leagues, ResourceAccessService access) : ControllerBase
{
    [HttpGet("{leagueId}")]
    public async Task<ActionResult<LeagueDetailDto>> Get(string leagueId)
    {
        if (!await Member(leagueId)) return NotFound();
        var league = await leagues.GetLeagueAsync(leagueId);
        if (league == null) return NotFound();
        var users = await leagues.GetLeagueUsersAsync(leagueId);
        var names = users.ToDictionary(user => user.Id, user => user.DisplayName ?? user.Id);
        var teams = await leagues.GetLeagueTeamsAsync(leagueId);
        var previous = league.PreviousLeagueId == null ? null : await leagues.GetLeagueAsync(league.PreviousLeagueId);
        return Ok(new LeagueDetailDto(MeController.ToLeague(league), teams.OrderByDescending(team => team.Wins).Select(team => MeController.ToTeam(team, team.OwnerId != null ? names.GetValueOrDefault(team.OwnerId) : null)).ToList(), previous == null ? null : MeController.ToLeague(previous)));
    }

    [HttpPost("{leagueId}/previous-season/import")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<LeagueDto>> ImportPreviousSeason(string leagueId)
    {
        if (!await Member(leagueId)) return NotFound();
        var league = await leagues.GetLeagueAsync(leagueId);
        if (league?.PreviousLeagueId == null) return NotFound();
        var previous = await leagues.ImportLeagueAsync(league.PreviousLeagueId);
        await leagues.ImportUsersAsync(previous.Id);
        await leagues.ImportRostersAsync(previous.Id);
        return Ok(MeController.ToLeague(previous));
    }

    [HttpGet("{leagueId}/teams/{teamId}")]
    public async Task<ActionResult<RosterDto>> Roster(string leagueId, string teamId)
    {
        if (!await Member(leagueId)) return NotFound();
        var league = await leagues.GetLeagueAsync(leagueId);
        var roster = await leagues.GetTeamRosterAsync(teamId);
        if (league == null || roster == null || roster.Team.LeagueId != leagueId) return NotFound();
        return Ok(new RosterDto(MeController.ToLeague(league), MeController.ToTeam(roster.Team, roster.ManagerName), roster.Starters.Select(MeController.ToPlayer).ToList(), roster.Bench.Select(MeController.ToPlayer).ToList(), roster.Reserve.Select(MeController.ToPlayer).ToList(), roster.Taxi.Select(MeController.ToPlayer).ToList()));
    }

    [HttpGet("{leagueId}/draft")]
    public async Task<ActionResult<DraftDto>> Draft(string leagueId)
    {
        if (!await Member(leagueId)) return NotFound();
        var league = await leagues.GetLeagueAsync(leagueId);
        var info = await leagues.GetDraftInfoAsync(leagueId);
        if (league == null || info == null) return NotFound();
        var teams = await leagues.GetLeagueTeamsAsync(leagueId);
        var userNames = (await leagues.GetLeagueUsersAsync(leagueId)).ToDictionary(user => user.Id, user => user.DisplayName ?? user.Id);
        var teamByRoster = teams.ToDictionary(team => team.Id.Split('_').Last(), team => team);
        var traded = (await leagues.GetTradedPicksAsync(leagueId)).Where(pick => pick.Season == league.Season.ToString()).ToDictionary(pick => (pick.RosterId, pick.Round), pick => pick.OwnerId);
        var teamDtos = teams.Select(team => MeController.ToTeam(team, team.OwnerId != null ? userNames.GetValueOrDefault(team.OwnerId) : null)).ToList();
        var picks = (await leagues.GetDraftPicksAsync(leagueId)).OrderBy(pick => pick.PickNumber).Select(pick =>
        {
            var team = pick.TeamId != null ? teamByRoster.GetValueOrDefault(pick.TeamId) : null;
            return new DraftPickDto(pick.Round, pick.PickNumber, pick.TeamId, team?.TeamName, team?.OwnerId != null ? userNames.GetValueOrDefault(team.OwnerId) : null, OriginalTeam(info, pick.Round, pick.PickNumber, pick.TeamId, teamByRoster), pick.PlayerName, pick.Position, pick.Team, pick.IsKeeper == true);
        }).ToList();
        var planned = BuildPlannedOrder(info, traded, teamByRoster, userNames);
        return Ok(new DraftDto(MeController.ToLeague(league), info.Status, info.Type, info.Status == "complete" ? picks.Count : (info.Rounds ?? 0) * (info.Teams ?? 0), teamDtos, picks, planned));
    }

    [HttpPost("{leagueId}/draft/import")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ImportDraft(string leagueId)
    {
        if (!await Member(leagueId)) return NotFound();
        await leagues.ImportDraftPicksAsync(leagueId);
        return NoContent();
    }

    private Task<bool> Member(string leagueId) => access.IsLeagueMemberAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, leagueId);
    private static string? OriginalTeam(Fanfoot.Domain.Models.DraftInfo info, int round, int pickNumber, string? actualTeamId, Dictionary<string, Fanfoot.Domain.Models.Team> teams)
    {
        if (actualTeamId == null || info.SlotToRosterId == null || !int.TryParse(actualTeamId, out var actual)) return null;
        var slots = info.SlotToRosterId.OrderBy(slot => int.Parse(slot.Key)).Select(slot => slot.Value).ToList();
        if (slots.Count == 0) return null;
        var index = (pickNumber - 1) % slots.Count;
        if (string.Equals(info.Type, "snake", StringComparison.OrdinalIgnoreCase) && round % 2 == 0) index = slots.Count - index - 1;
        return slots[index] == actual ? null : teams.GetValueOrDefault(slots[index].ToString())?.TeamName;
    }
    private static List<DraftOrderDto> BuildPlannedOrder(Fanfoot.Domain.Models.DraftInfo info, Dictionary<(int, int), int> traded, Dictionary<string, Fanfoot.Domain.Models.Team> teams, Dictionary<string, string> names)
    {
        var source = info.SlotToRosterId?.OrderBy(slot => int.Parse(slot.Key)).Select(slot => slot.Value).ToList() ?? [];
        var output = new List<DraftOrderDto>();
        for (var round = 1; round <= (info.Rounds ?? 0); round++)
            foreach (var original in (string.Equals(info.Type, "snake", StringComparison.OrdinalIgnoreCase) && round % 2 == 0 ? source.AsEnumerable().Reverse() : source))
            {
                var owner = traded.GetValueOrDefault((original, round), original);
                var team = teams.GetValueOrDefault(owner.ToString());
                output.Add(new DraftOrderDto(round, output.Count + 1, owner.ToString(), team?.TeamName ?? $"Team {owner}", team?.OwnerId != null ? names.GetValueOrDefault(team.OwnerId, "-") : "-", owner == original ? null : teams.GetValueOrDefault(original.ToString())?.TeamName));
            }
        return output;
    }
}
