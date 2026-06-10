using Fanfoot.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fanfoot.Web.Controllers;

public record ImportPlayersResponse(int Imported);

[ApiController]
[Route("api/players")]
public class PlayersController : ControllerBase
{
    private readonly LeagueService _leagueService;

    public PlayersController(LeagueService leagueService) => _leagueService = leagueService;

    [HttpPost("import")]
    [Authorize]
    public async Task<ActionResult<ImportPlayersResponse>> Import()
    {
        var count = await _leagueService.ImportPlayersAsync();
        return Ok(new ImportPlayersResponse(count));
    }
}
