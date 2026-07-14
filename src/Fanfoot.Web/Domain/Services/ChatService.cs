using System.Text.Json;
using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Clients;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Data.Entities;
using Fanfoot.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Fanfoot.Domain.Services;

public class ChatService
{
    private readonly FanfootDbContext _db;
    private readonly SleeperClient _sleeper;
    private readonly FantasyCalcClient _fantasyCalc;
    private readonly EspnClient _espn;
    private readonly LlmClient _llm;
    private readonly ILogger<ChatService> _logger;

    // FantasyCalc value cache — scoped to circuit lifetime, keyed per league
    private string? _valuesCachedForLeague;
    private Dictionary<string, FantasyCalcValueDto> _valuesBySleeperId = [];
    private Dictionary<string, FantasyCalcValueDto> _valuesByName = [];

    public ChatService(
        FanfootDbContext db,
        SleeperClient sleeper,
        FantasyCalcClient fantasyCalc,
        EspnClient espn,
        LlmClient llm,
        ILogger<ChatService> logger)
    {
        _db = db;
        _sleeper = sleeper;
        _fantasyCalc = fantasyCalc;
        _espn = espn;
        _llm = llm;
        _logger = logger;
    }

    public async Task<string> GetUserContextAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return "";

        var teams = await _db.Teams.Where(t => t.OwnerId == userId).ToListAsync();
        var leagueIds = teams.Select(t => t.LeagueId).Distinct().ToList();
        var leagues = await _db.Leagues.Where(l => leagueIds.Contains(l.Id)).ToListAsync();
        var leagueMap = leagues.ToDictionary(l => l.Id);

        var playerIds = teams
            .Where(t => !string.IsNullOrEmpty(t.Roster))
            .SelectMany(t => JsonSerializer.Deserialize<List<string>>(t.Roster!) ?? [])
            .Distinct().ToList();

        var players = playerIds.Count > 0
            ? await _db.Players.Where(p => playerIds.Contains(p.Id)).ToListAsync()
            : [];
        var playerMap = players.ToDictionary(p => p.Id);

        var context = $"You are an AI assistant for a fantasy football tracker app. The current user is {user.DisplayName}.";
        context += $"\n\nThey have {teams.Count} team(s) across {leagues.Count} league(s):\n";

        foreach (var team in teams)
        {
            var league = leagueMap.GetValueOrDefault(team.LeagueId);
            context += $"\n--- {team.TeamName} ({league?.Name ?? "?"}, {league?.Season.ToString() ?? "?"}) | {team.Wins}-{team.Losses}-{team.Ties} | PF: {team.PointsFor:F1} ---";

            var starterIds = JsonSerializer.Deserialize<List<string>>(team.Starters ?? "[]") ?? [];
            var rosterIds = JsonSerializer.Deserialize<List<string>>(team.Roster ?? "[]") ?? [];

            if (starterIds.Count > 0)
            {
                var names = starterIds.Select(id => playerMap.GetValueOrDefault(id)).OfType<PlayerEntity>()
                    .Select(p => $"{p.FirstName} {p.LastName} ({p.Position})");
                context += $"\n  Starters: {string.Join(", ", names)}";
            }

            var benchIds = rosterIds.Except(starterIds).ToList();
            if (benchIds.Count > 0)
            {
                var names = benchIds.Select(id => playerMap.GetValueOrDefault(id)).OfType<PlayerEntity>()
                    .Select(p => $"{p.FirstName} {p.LastName} ({p.Position})");
                context += $"\n  Bench: {string.Join(", ", names)}";
            }
        }

        return context;
    }

    public async Task<string> GetLeagueContextAsync(string leagueId, string? viewerUserId = null)
    {
        var league = await _db.Leagues.FindAsync(leagueId);
        if (league == null) return "";

        var teams = await _db.Teams.Where(t => t.LeagueId == leagueId).OrderByDescending(t => t.Wins).ToListAsync();
        var users = await _db.Users.Where(u => u.LeagueId == leagueId).ToListAsync();
        var userMap = users.ToDictionary(u => u.Id, u => u.DisplayName ?? u.Id);

        var allPlayerIds = teams
            .SelectMany(t =>
            {
                var roster = JsonSerializer.Deserialize<List<string>>(t.Roster ?? "[]") ?? [];
                var reserve = JsonSerializer.Deserialize<List<string>>(t.Reserve ?? "[]") ?? [];
                return roster.Concat(reserve);
            })
            .Distinct().ToList();

        var players = allPlayerIds.Count > 0
            ? await _db.Players.Where(p => allPlayerIds.Contains(p.Id)).ToListAsync()
            : [];
        var playerMap = players.ToDictionary(p => p.Id);

        var (settingsContext, isDynasty, isPpr, numQbs) = await BuildSettingsAsync(leagueId, league);
        await EnsurePlayerValuesAsync(leagueId, isDynasty, isPpr, numQbs, league.TotalRosters);

        // Fetch current week and matchup from Sleeper
        var state = await _sleeper.GetNflStateAsync();
        var currentWeek = state?.Week ?? 0;
        TeamEntity? userTeam = null;
        TeamEntity? opponentTeam = null;

        if (viewerUserId != null)
        {
            userTeam = teams.FirstOrDefault(t => t.OwnerId == viewerUserId);

            if (userTeam != null && currentWeek > 0)
            {
                try
                {
                    var weekMatchups = await _sleeper.GetLeagueMatchupsAsync(leagueId, currentWeek);
                    var userRosterId = int.Parse(userTeam.Id.Split('_').Last());
                    var userMatchup = weekMatchups?.FirstOrDefault(m => m.RosterId == userRosterId);
                    if (userMatchup != null)
                    {
                        var opponentMatchup = weekMatchups!.FirstOrDefault(m =>
                            m.MatchupId == userMatchup.MatchupId && m.RosterId != userRosterId);
                        if (opponentMatchup != null)
                            opponentTeam = teams.FirstOrDefault(t =>
                                t.Id.Split('_').Last() == opponentMatchup.RosterId.ToString());
                    }
                }
                catch { /* matchup data unavailable */ }
            }
        }

        // ── System prompt ──────────────────────────────────────────────────────
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("You are a sharp, knowledgeable fantasy football advisor — like a friend who knows the game deeply and gives real, direct advice. Keep your tone conversational and confident. Never be vague or generic.");
        sb.AppendLine();
        sb.AppendLine("RULES:");
        sb.AppendLine("- Always ground your advice in the specific players and rosters listed below. Never invent players or teams not present in the data.");
        sb.AppendLine("- Never quote numerical trade values or rankings. Describe players qualitatively: 'elite WR1', 'mid-tier RB2', 'sell-high candidate', etc.");
        sb.AppendLine("- Always consider the user's full roster. Don't suggest trading away a starter unless the bench can cover that spot.");
        sb.AppendLine("- When suggesting trades, name real players from real teams already listed in this league.");
        sb.AppendLine("- Keep responses focused and actionable. No padding.");
        sb.AppendLine();
        sb.AppendLine("TOOL USAGE — tools exist for real-time lookups only. Default answer is: call no tools.");
        sb.AppendLine("- get_player_recent_stats: ONLY when the user's message explicitly asks how a specific named player has been performing recently (e.g. 'how has X been doing?', 'show me X's last few weeks'). Do NOT call this speculatively or to 'support' a trade argument.");
        sb.AppendLine("- get_player_news: ONLY when the user's message explicitly asks about news, injury, or status on a specific named player (e.g. 'any news on X?', 'is X injured?'). Do NOT call this speculatively.");
        sb.AppendLine("- Trade values and player rankings are ALREADY embedded in the roster data below. Do NOT call any tool to look up values or rankings.");
        sb.AppendLine("- For ALL trade help, lineup decisions, roster analysis, and 'would they accept?' questions: reason from the data provided. Call zero tools.");
        sb.AppendLine();
        sb.AppendLine("TRADE REQUEST WORKFLOW — when the user asks for trade help:");
        sb.AppendLine("1. Identify the user's roster gaps (positional weaknesses, thin depth, injury risks) using the roster data below");
        sb.AppendLine("2. Identify real trade assets the user can offer: players that are surplus, sell-high, or redundant given their depth. The tier labels in the roster data reflect relative value — only offer players whose tier is comparable to what you're asking for in return.");
        if (isDynasty)
        {
            sb.AppendLine("   - DYNASTY: Draft picks are tradeable assets for the annual ROOKIE DRAFT only — they select current-year rookies (YearsExp=0), not veterans. Veterans not on a roster are waiver-wire claims, not draftable. Use the pick holdings below; a 1st from a weak team (projected top-3 pick) is worth far more than a 1st from a contender.");
            sb.AppendLine("   - Consider packaging picks + players to bridge value gaps. A high rookie pick can be the difference-maker when the user can't match a target's player value alone.");
            sb.AppendLine("   - The ROOKIES AVAILABLE section below shows who could be had with those picks. Factor that into whether a pick is worth trading away.");
        }
        sb.AppendLine("3. FAIRNESS CHECK (critical): For every proposed trade, verify it is realistic. A 'bench-level' or 'low-end' player cannot fetch an 'elite' or 'top-3' player. The value of what the user gives must roughly match the value of what they receive — use the tier labels to compare. If the user doesn't have enough surplus value to get what they need, say so honestly and suggest realistic alternatives.");
        sb.AppendLine("4. Find specific players on OTHER teams in this league that address those gaps AND that the other manager might realistically want to trade away (e.g. they have surplus depth at that position, or the player is aging/injured)");
        sb.AppendLine("5. Propose 2-3 concrete trade packages. For each, explicitly state why BOTH sides would benefit — what does the other manager gain that they need?");
        sb.AppendLine();

        // ── Current situation ──────────────────────────────────────────────────
        sb.AppendLine($"LEAGUE: {league.Name} ({league.Season} season){settingsContext}");
        sb.AppendLine();

        if (userTeam != null)
        {
            sb.AppendLine($"THE USER'S TEAM: {userTeam.TeamName} | Record: {userTeam.Wins}-{userTeam.Losses}-{userTeam.Ties} | PF: {userTeam.PointsFor:F1} | PA: {userTeam.PointsAgainst:F1}");
            AppendRoster(sb, userTeam, playerMap, isDynasty, "User's");
            sb.AppendLine();
        }

        if (opponentTeam != null)
        {
            sb.AppendLine($"THIS WEEK'S OPPONENT (Week {currentWeek}): {opponentTeam.TeamName} | Record: {opponentTeam.Wins}-{opponentTeam.Losses}-{opponentTeam.Ties} | PF: {opponentTeam.PointsFor:F1} | PA: {opponentTeam.PointsAgainst:F1}");
            AppendRoster(sb, opponentTeam, playerMap, isDynasty, "Opponent's");
            sb.AppendLine();
        }
        else if (currentWeek > 0)
        {
            sb.AppendLine($"Current week: {currentWeek}");
        }

        // ── Rest of league ─────────────────────────────────────────────────────
        sb.AppendLine("REST OF LEAGUE (standings and rosters):");
        foreach (var team in teams.Where(t => t != userTeam && t != opponentTeam))
        {
            var manager = team.OwnerId != null && userMap.TryGetValue(team.OwnerId, out var mgr) ? mgr : "Unknown";
            sb.AppendLine($"\n{team.TeamName} (manager: {manager}) | {team.Wins}-{team.Losses}-{team.Ties} | PF: {team.PointsFor:F1}");
            AppendRoster(sb, team, playerMap, isDynasty, null);
        }

        if (isDynasty)
            sb.Append(await BuildDynastyPicksContextAsync(leagueId, teams));
        else
            sb.Append(await BuildPicksContextAsync(leagueId, teams));
        sb.Append(await BuildFreeAgentsContextAsync(allPlayerIds, isDynasty));
        return sb.ToString();
    }

    private void AppendRoster(System.Text.StringBuilder sb, TeamEntity team, Dictionary<string, PlayerEntity> playerMap, bool isDynasty, string? label)
    {
        var starterIds = JsonSerializer.Deserialize<List<string>>(team.Starters ?? "[]") ?? [];
        var rosterIds = JsonSerializer.Deserialize<List<string>>(team.Roster ?? "[]") ?? [];
        var reserveIds = JsonSerializer.Deserialize<List<string>>(team.Reserve ?? "[]") ?? [];
        var benchIds = rosterIds.Except(starterIds).ToList();

        if (starterIds.Count > 0)
        {
            var names = starterIds.Select(id => playerMap.GetValueOrDefault(id)).OfType<PlayerEntity>()
                .Select(p => $"{p.FirstName} {p.LastName} ({p.Position}, {p.Team ?? "FA"}{PlayerValueSuffix(p.Id, isDynasty)})");
            sb.AppendLine($"  Starters: {string.Join(", ", names)}");
        }
        if (benchIds.Count > 0)
        {
            var names = benchIds.Select(id => playerMap.GetValueOrDefault(id)).OfType<PlayerEntity>()
                .Select(p => $"{p.FirstName} {p.LastName} ({p.Position}, {p.Team ?? "FA"}{PlayerValueSuffix(p.Id, isDynasty)})");
            sb.AppendLine($"  Bench: {string.Join(", ", names)}");
        }
        if (reserveIds.Count > 0)
        {
            var names = reserveIds.Select(id => playerMap.GetValueOrDefault(id)).OfType<PlayerEntity>()
                .Select(p => $"{p.FirstName} {p.LastName} ({p.Position}, {p.Team ?? "FA"})");
            sb.AppendLine($"  IR: {string.Join(", ", names)}");
        }
    }

    public async Task<string> AskAsync(string systemPrompt, List<(string Role, string Content)> history)
    {
        _logger.LogInformation("AskAsync: model={Model} systemPromptLength={Length} historyCount={Count}",
            _llm.Model, systemPrompt?.Length ?? 0, history.Count);

        var messages = new List<LlmMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };
        messages.AddRange(history.Select(h => new LlmMessage { Role = h.Role, Content = h.Content }));

        var tools = BuildTools();

        for (int i = 0; i < 3; i++)
        {
            var msg = await _llm.ChatAsync(messages, tools);

            if (msg?.ToolCalls is not { Count: > 0 })
                return CleanResponse(msg?.Content);

            messages.Add(msg);

            foreach (var toolCall in msg.ToolCalls)
            {
                JsonElement args = default;
                if (!string.IsNullOrEmpty(toolCall.Function?.Arguments))
                {
                    try { args = JsonDocument.Parse(toolCall.Function.Arguments).RootElement; }
                    catch { }
                }
                var toolResult = await ExecuteToolAsync(toolCall.Function?.Name, args);
                messages.Add(new LlmMessage { Role = "tool", ToolCallId = toolCall.Id, Content = toolResult });
            }
        }

        return "Sorry, the request required too many steps to complete.";
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    private async Task<(string Context, bool IsDynasty, bool IsPpr, int NumQbs)> BuildSettingsAsync(string leagueId, LeagueEntity league)
    {
        var dto = await _sleeper.GetLeagueAsync(leagueId);
        if (dto == null) return ("", false, false, 1);

        var sb = new System.Text.StringBuilder("\n\nLeague settings:");

        var isDynasty = false;
        var leagueType = "Redraft";
        if (dto.Settings?.TryGetValue("type", out var typeVal) == true)
        {
            leagueType = typeVal?.ToString() switch { "1" => "Keeper", "2" => "Dynasty", _ => "Redraft" };
            isDynasty = leagueType == "Dynasty";
        }
        else if (dto.Settings?.TryGetValue("taxi_slots", out var taxiVal) == true
                 && int.TryParse(taxiVal?.ToString(), out var taxi) && taxi > 0)
        {
            leagueType = "Dynasty"; isDynasty = true;
        }
        sb.Append($"\n  Type: {leagueType}");

        var isPpr = false;
        if (dto.ScoringSettings != null)
        {
            var recStr = dto.ScoringSettings.TryGetValue("rec", out var rec) ? rec?.ToString() : null;
            var scoring = recStr switch { "1" => "PPR", "0.5" => "Half-PPR", _ => "Standard" };
            isPpr = recStr == "1";

            if (dto.ScoringSettings.TryGetValue("bonus_rec_te", out var te)
                && double.TryParse(te?.ToString(), out var tePrem) && tePrem > 0)
                scoring += $" + TE Premium (+{tePrem}/rec)";

            sb.Append($"\n  Scoring: {scoring}");
        }

        var numQbs = 1;
        if (dto.RosterPositions != null && dto.RosterPositions.Count > 0)
        {
            var starters = dto.RosterPositions.Where(p => p != "BN" && p != "IR" && p != "TAXI").ToList();
            sb.Append($"\n  Starting positions: {string.Join(", ", starters)}");

            if (dto.RosterPositions.Contains("SUPER_FLEX")) sb.Append("\n  Superflex: Yes");

            numQbs = dto.RosterPositions.Count(p => p == "QB");
            if (dto.RosterPositions.Contains("SUPER_FLEX")) numQbs++;
            if (numQbs >= 2) sb.Append($"\n  2QB/Superflex: Yes");

            var taxiSlots = dto.RosterPositions.Count(p => p == "TAXI");
            if (taxiSlots > 0) sb.Append($"\n  Taxi squad: {taxiSlots} slots");

            var irSlots = dto.RosterPositions.Count(p => p == "IR");
            if (irSlots > 0) sb.Append($"\n  IR slots: {irSlots}");

            sb.Append($"\n  Bench slots: {dto.RosterPositions.Count(p => p == "BN")}");
        }

        if (dto.Settings?.TryGetValue("best_ball", out var bb) == true && bb?.ToString() == "1")
            sb.Append("\n  Best ball: Yes");

        if (dto.Settings?.TryGetValue("max_keepers", out var mk) == true
            && int.TryParse(mk?.ToString(), out var maxK) && maxK > 0)
            sb.Append($"\n  Max keepers: {maxK}");

        return (sb.ToString(), isDynasty, isPpr, numQbs);
    }

    // ── FantasyCalc values ────────────────────────────────────────────────────

    private async Task EnsurePlayerValuesAsync(string leagueId, bool isDynasty, bool isPpr, int numQbs, int numTeams)
    {
        if (_valuesCachedForLeague == leagueId) return;

        try
        {
            var values = await _fantasyCalc.GetValuesAsync(isDynasty, numQbs, isPpr, numTeams);
            if (values == null) return;

            _valuesBySleeperId = values
                .Where(v => v.Player?.SleeperId != null)
                .ToDictionary(v => v.Player!.SleeperId!, v => v);

            _valuesByName = values
                .Where(v => v.Player?.Name != null)
                .GroupBy(v => NormalizeName(v.Player!.Name))
                .ToDictionary(g => g.Key, g => g.First());

            _valuesCachedForLeague = leagueId;
        }
        catch { /* FantasyCalc unavailable — proceed without values */ }
    }

    private string PlayerValueSuffix(string sleeperId, bool isDynasty)
    {
        if (!_valuesBySleeperId.TryGetValue(sleeperId, out var val)) return "";

        var pos = val.Player?.Position ?? "";
        var tier = val.PositionRank switch
        {
            1     => $"the #1 {pos}",
            <= 3  => $"top-3 {pos}",
            <= 6  => $"top-6 {pos}",
            <= 12 => $"solid {pos}1",
            <= 24 => $"mid-tier {pos}2",
            <= 36 => $"low-end {pos}3",
            _     => $"bench-level {pos}"
        };

        var trend = val.Trend30Day switch
        {
            > 300  => ", rising",
            < -300 => ", falling",
            _      => ""
        };

        return $", {tier}{trend}";
    }

    // ── Picks context ─────────────────────────────────────────────────────────

    private async Task<string> BuildPicksContextAsync(string leagueId, List<TeamEntity> teams)
    {
        var tradedPicks = await _sleeper.GetTradedPicksAsync(leagueId);
        if (tradedPicks == null || tradedPicks.Count == 0) return "";

        var rosterToName = teams.ToDictionary(
            t => int.Parse(t.Id.Split('_').Last()),
            t => t.TeamName);

        var acquired = tradedPicks.Where(p => p.RosterId != p.OwnerId)
            .GroupBy(p => p.RosterId).ToDictionary(g => g.Key, g => g.ToList());

        var tradedAway = tradedPicks.Where(p => p.RosterId != p.OwnerId)
            .GroupBy(p => p.OwnerId).ToDictionary(g => g.Key, g => g.ToList());

        if (acquired.Count == 0) return "";

        var sb = new System.Text.StringBuilder("\n\nDraft pick ownership (traded picks):\n");
        foreach (var rosterId in acquired.Keys.Union(tradedAway.Keys).OrderBy(x => x))
        {
            var teamName = rosterToName.GetValueOrDefault(rosterId, $"Roster {rosterId}");
            sb.Append($"\n{teamName}:");

            if (acquired.TryGetValue(rosterId, out var got))
            {
                var desc = got.OrderBy(p => p.Season).ThenBy(p => p.Round)
                    .Select(p => $"{p.Season} R{p.Round} (from {rosterToName.GetValueOrDefault(p.OwnerId, $"Roster {p.OwnerId}")})");
                sb.Append($" acquired {string.Join(", ", desc)};");
            }
            if (tradedAway.TryGetValue(rosterId, out var gave))
            {
                var desc = gave.OrderBy(p => p.Season).ThenBy(p => p.Round)
                    .Select(p => $"{p.Season} R{p.Round} (to {rosterToName.GetValueOrDefault(p.RosterId, $"Roster {p.RosterId}")})");
                sb.Append($" traded away {string.Join(", ", desc)};");
            }
        }
        return sb.ToString();
    }

    private async Task<string> BuildDynastyPicksContextAsync(string leagueId, List<TeamEntity> teams)
    {
        var tradedPicks = await _sleeper.GetTradedPicksAsync(leagueId);
        var state = await _sleeper.GetNflStateAsync();

        int baseYear = DateTime.UtcNow.Year;
        if (state?.Season != null && int.TryParse(state.Season, out var sy)) baseYear = sy;

        var rosterIds = teams.Select(t => int.Parse(t.Id.Split('_').Last())).ToList();
        var rosterToName = teams.ToDictionary(t => int.Parse(t.Id.Split('_').Last()), t => t.TeamName);

        var seasons = Enumerable.Range(baseYear, 3).Select(y => y.ToString()).ToList();
        int maxRound = tradedPicks?.Any() == true ? Math.Max(tradedPicks.Max(p => p.Round), 4) : 4;

        // Current ownership: last entry per (season, round, originalRosterId) wins
        var currentOwner = new Dictionary<(string, int, int), int>();
        if (tradedPicks != null)
            foreach (var pick in tradedPicks)
                currentOwner[(pick.Season, pick.Round, pick.RosterId)] = pick.OwnerId;

        // Build complete pick holdings per team
        var holdings = rosterIds.ToDictionary(id => id, _ => new List<string>());
        foreach (var season in seasons)
            for (int round = 1; round <= maxRound; round++)
                foreach (var rosterId in rosterIds)
                {
                    var holder = currentOwner.TryGetValue((season, round, rosterId), out var owner) ? owner : rosterId;
                    if (holdings.ContainsKey(holder))
                        holdings[holder].Add($"{season} R{round}");
                }

        // Estimate draft order from standings (worst record = earliest pick)
        var draftPos = teams
            .OrderBy(t => t.Wins).ThenBy(t => t.PointsFor)
            .Select((t, i) => (RosterId: int.Parse(t.Id.Split('_').Last()), Pos: i + 1))
            .ToDictionary(x => x.RosterId, x => x.Pos);

        var sb = new System.Text.StringBuilder("\n\nDRAFT PICK HOLDINGS (dynasty rookie draft — picks are used to select current-year rookies only):");
        sb.AppendLine($"\n(Estimated {baseYear} draft order based on current standings: #1 = worst record, i.e. likely the best rookie prospect)");

        foreach (var rosterId in rosterIds.OrderBy(id => rosterToName.GetValueOrDefault(id, "")))
        {
            var teamName = rosterToName.GetValueOrDefault(rosterId, $"Roster {rosterId}");
            var pickList = holdings[rosterId];
            var pos = draftPos.TryGetValue(rosterId, out var p) ? $" [est. #{p} pick in {baseYear}]" : "";
            sb.Append($"\n  {teamName}{pos}: {(pickList.Count > 0 ? string.Join(", ", pickList.OrderBy(x => x)) : "no picks held")}");
        }

        return sb.ToString();
    }

    // ── Free agents ───────────────────────────────────────────────────────────

    private async Task<string> BuildFreeAgentsContextAsync(List<string> rosteredIds, bool isDynasty)
    {
        if (_valuesBySleeperId.Count == 0) return "";

        // Use valued player IDs as the filter set to avoid EF/span issues with array.Contains
        var rosteredSet = rosteredIds.ToHashSet(StringComparer.Ordinal);
        var candidateIds = _valuesBySleeperId.Keys.Where(id => !rosteredSet.Contains(id)).ToList();
        if (candidateIds.Count == 0) return "";

        var freeAgents = await _db.Players
            .Where(p => candidateIds.Contains(p.Id) &&
                        (p.Position == "QB" || p.Position == "RB" || p.Position == "WR" || p.Position == "TE"))
            .ToListAsync();

        var scored = freeAgents
            .Select(p => (Player: p, Val: _valuesBySleeperId.GetValueOrDefault(p.Id)))
            .Where(x => x.Val != null)
            .ToList();

        if (scored.Count == 0) return "";

        var sb = new System.Text.StringBuilder();

        if (isDynasty)
        {
            // Rookies (YearsExp == 0) are draftable with picks; veterans are waiver-only
            var rookies = scored
                .Where(x => x.Player.YearsExp == 0)
                .OrderBy(x => x.Val!.OverallRank)
                .Take(15)
                .ToList();

            var veterans = scored
                .Where(x => x.Player.YearsExp != 0)
                .OrderBy(x => x.Val!.PositionRank)
                .GroupBy(x => x.Player.Position)
                .SelectMany(g => g.Take(3))
                .OrderBy(x => x.Val!.OverallRank)
                .ToList();

            if (rookies.Count > 0)
            {
                sb.AppendLine("\n\nROOKIES AVAILABLE (draftable with picks OR waivered if undrafted):");
                foreach (var (player, val) in rookies)
                    sb.Append($"\n  {player.FirstName} {player.LastName} ({player.Position}, {player.Team ?? "?"}{PlayerValueSuffix(player.Id, true)})");
            }

            if (veterans.Count > 0)
            {
                sb.AppendLine("\n\nVETERAN FREE AGENTS (waiver wire only — NOT draftable):");
                foreach (var (player, val) in veterans)
                    sb.Append($"\n  {player.FirstName} {player.LastName} ({player.Position}, {player.Team ?? "FA"}{PlayerValueSuffix(player.Id, true)})");
            }
        }
        else
        {
            var notable = scored
                .OrderBy(x => x.Val!.PositionRank)
                .GroupBy(x => x.Player.Position)
                .SelectMany(g => g.Take(5))
                .OrderBy(x => x.Val!.OverallRank)
                .ToList();

            sb.AppendLine("\n\nNOTABLE FREE AGENTS (available to add/drop):");
            foreach (var (player, val) in notable)
                sb.Append($"\n  {player.FirstName} {player.LastName} ({player.Position}, {player.Team ?? "FA"}{PlayerValueSuffix(player.Id, false)})");
        }

        return sb.ToString();
    }

    // ── Tool definitions ──────────────────────────────────────────────────────

    private static List<LlmTool> BuildTools() =>
    [
        new()
        {
            Function = new()
            {
                Name = "get_player_recent_stats",
                Description = "Get recent weekly fantasy stats (points, yards, TDs, receptions) for a player.",
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        player_name = new { type = "string", description = "Full name of the NFL player" }
                    },
                    required = new[] { "player_name" }
                }
            }
        },
        new()
        {
            Function = new()
            {
                Name = "get_player_news",
                Description = "Search for recent NFL news articles and injury updates about a specific player.",
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        player_name = new { type = "string", description = "Full name of the NFL player" }
                    },
                    required = new[] { "player_name" }
                }
            }
        }
    ];

    // ── Tool execution ────────────────────────────────────────────────────────

    private Task<string> ExecuteToolAsync(string? name, JsonElement? args) => name switch
    {
        "get_player_trade_value" => ToolGetPlayerTradeValue(args ?? default),
        "get_player_recent_stats" => ToolGetPlayerRecentStats(args ?? default),
        "get_player_news" => ToolGetPlayerNews(args ?? default),
        _ => Task.FromResult("Unknown tool.")
    };

    private Task<string> ToolGetPlayerTradeValue(JsonElement args)
    {
        var name = args.TryGetProperty("player_name", out var np) ? np.GetString() ?? "" : "";
        if (string.IsNullOrEmpty(name)) return Task.FromResult("Player name required.");

        var key = NormalizeName(name);
        if (!_valuesByName.TryGetValue(key, out var val))
        {
            val = _valuesByName
                .Where(kvp => kvp.Key.Contains(key) || key.Contains(kvp.Key))
                .OrderBy(kvp => Math.Abs(kvp.Key.Length - key.Length))
                .FirstOrDefault().Value;
        }

        if (val == null) return Task.FromResult($"No trade value data found for '{name}'.");

        var totalPlayers = _valuesByName.Count;
        var overallTier = val.OverallRank switch
        {
            <= 12 => "elite overall value (top 12 in the league format)",
            <= 30 => "high overall value (top 30)",
            <= 60 => "solid mid-range overall value",
            <= 100 => "below-average overall value",
            _ => "low overall value"
        };

        var posTier = val.PositionRank switch
        {
            1 => "the #1 player at their position",
            <= 3 => "a top-3 player at their position",
            <= 6 => "a top-6 player at their position",
            <= 12 => "a strong starter at their position",
            <= 24 => "a mid-tier player at their position",
            _ => "a low-value player at their position"
        };

        var trend = val.Trend30Day switch
        {
            > 300 => "trending up significantly over the past month",
            > 100 => "trending up slightly over the past month",
            < -300 => "trending down significantly over the past month — sell-high candidate",
            < -100 => "trending down slightly over the past month",
            _ => "stable value"
        };

        var dynastyVsRedraft = (val.RedraftValue.HasValue && val.Value > 0)
            ? (val.Value > val.RedraftValue * 1.2 ? "more valuable in dynasty than redraft" :
               val.RedraftValue > val.Value * 1.2 ? "more valuable in redraft than dynasty" :
               "similar value in dynasty and redraft")
            : "";

        return Task.FromResult(
            $"{val.Player?.Name ?? name} ({val.Player?.Position}, {val.Player?.MaybeTeam ?? "?"}) — " +
            $"{overallTier}; {posTier}; {trend}" +
            (dynastyVsRedraft.Length > 0 ? $"; {dynastyVsRedraft}" : ""));
    }

    private async Task<string> ToolGetPlayerRecentStats(JsonElement args)
    {
        var name = args.TryGetProperty("player_name", out var np) ? np.GetString() ?? "" : "";
        var weeks = args.TryGetProperty("weeks", out var wp) && wp.TryGetInt32(out var w) ? Math.Clamp(w, 1, 8) : 4;
        if (string.IsNullOrEmpty(name)) return "Player name required.";

        var nameParts = name.Split(' ', 2);
        var player = nameParts.Length == 2
            ? await _db.Players.FirstOrDefaultAsync(p =>
                p.FirstName.ToLower() == nameParts[0].ToLower() &&
                p.LastName.ToLower() == nameParts[1].ToLower())
            : await _db.Players.FirstOrDefaultAsync(p =>
                (p.FirstName + " " + p.LastName).ToLower().Contains(name.ToLower()));

        if (player == null) return $"Player '{name}' not found.";

        var state = await _sleeper.GetNflStateAsync();
        if (state == null) return "Could not retrieve current NFL season info.";

        var results = new List<string>();
        for (int i = 0; i < weeks && state.Week - i > 0; i++)
        {
            var week = state.Week - i;
            var stats = await _sleeper.GetWeeklyStatsAsync(state.Season, week);
            if (stats == null || !stats.TryGetValue(player.Id, out var ps))
            {
                results.Add($"Week {week}: no data");
                continue;
            }

            double Get(string k)
            {
                if (!ps.TryGetValue(k, out var v)) return 0;
                if (v.ValueKind == JsonValueKind.Number) return v.GetDouble();
                if (v.ValueKind == JsonValueKind.String && double.TryParse(v.GetString(), out var parsed)) return parsed;
                return 0;
            }

            var pts = Get("pts_ppr");
            var statParts = new List<string>();

            if (player.Position == "QB")
            {
                if (Get("pass_yd") > 0) statParts.Add($"{(int)Get("pass_yd")} pass yds, {(int)Get("pass_td")} TD, {(int)Get("pass_int")} INT");
                if (Get("rush_yd") > 0) statParts.Add($"{(int)Get("rush_yd")} rush yds");
            }
            else if (player.Position == "RB")
            {
                if (Get("rush_yd") > 0) statParts.Add($"{(int)Get("rush_yd")} rush yds, {(int)Get("rush_td")} TD");
                if (Get("rec") > 0) statParts.Add($"{(int)Get("rec")} rec, {(int)Get("rec_yd")} yds");
            }
            else
            {
                if (Get("rec") > 0) statParts.Add($"{(int)Get("rec")} rec, {(int)Get("rec_yd")} yds, {(int)Get("rec_td")} TD");
            }

            results.Add($"Week {week}: {pts:F1} pts" + (statParts.Count > 0 ? $" ({string.Join(", ", statParts)})" : ""));
        }

        return $"Recent stats for {player.FirstName} {player.LastName} ({player.Position}, {player.Team ?? "FA"}):\n"
               + string.Join("\n", results);
    }

    private async Task<string> ToolGetPlayerNews(JsonElement args)
    {
        var name = args.TryGetProperty("player_name", out var np) ? np.GetString() ?? "" : "";
        if (string.IsNullOrEmpty(name)) return "Player name required.";

        try
        {
            var response = await _espn.GetNewsAsync();

            if (response?.Articles == null) return "Could not retrieve NFL news.";

            var nameLower = name.ToLowerInvariant();
            var matching = response.Articles
                .Where(a => a.Athletes?.Any(ath => ath.DisplayName?.ToLowerInvariant().Contains(nameLower) == true) == true)
                .Take(5).ToList();

            if (matching.Count == 0)
                return $"No recent news found for '{name}'.";

            var items = matching.Select(a =>
            {
                var date = DateTime.TryParse(a.Published, out var d) ? d.ToString("MMM d") : "";
                return $"[{date}] {a.Headline}" + (!string.IsNullOrEmpty(a.Description) ? $": {a.Description}" : "");
            });

            return $"Recent news for {name}:\n" + string.Join("\n", items);
        }
        catch (Exception ex)
        {
            return $"Could not retrieve news: {ex.Message}";
        }
    }

    // ── Chat history ─────────────────────────────────────────────────────────

    public async Task<List<ChatSession>> GetSessionsAsync(string userId)
    {
        var sessions = await _db.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .Take(10)
            .ToListAsync();
        return sessions.Select(EntityMapper.ToDomain).ToList();
    }

    public async Task<ChatSession?> GetSessionAsync(string userId, string sessionId)
    {
        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        return session == null ? null : EntityMapper.ToDomain(session);
    }

    public async Task SaveSessionAsync(
        string sessionId, string userId, string? leagueId,
        string title, List<(string Role, string Content)> history)
    {
        var session = await _db.ChatSessions.FindAsync(sessionId);
        if (session != null && session.UserId != userId)
            throw new UnauthorizedAccessException("Chat session is not owned by this user.");
        if (session == null)
        {
            session = new ChatSessionEntity { Id = sessionId, UserId = userId, LeagueId = leagueId, CreatedAt = DateTimeOffset.UtcNow };
            _db.ChatSessions.Add(session);
        }

        session.Title = title.Length > 100 ? title[..100] : title;
        session.LeagueId = leagueId;
        session.MessagesJson = JsonSerializer.Serialize(
            history.Select(h => new { role = h.Role, content = h.Content }));
        session.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        // Enforce 10-session limit per user
        var overflow = await _db.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .Skip(10)
            .ToListAsync();
        if (overflow.Count > 0)
        {
            _db.ChatSessions.RemoveRange(overflow);
            await _db.SaveChangesAsync();
        }
    }

    public static List<(string Role, string Content)> DeserializeHistory(string messagesJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(messagesJson);
            return doc.RootElement.EnumerateArray()
                .Select(e => (
                    Role: e.GetProperty("role").GetString() ?? "user",
                    Content: e.GetProperty("content").GetString() ?? ""))
                .ToList();
        }
        catch { return []; }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string NormalizeName(string name) =>
        new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static string CleanResponse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "Sorry, I couldn't process that.";
        var idx = content.IndexOf("<|python_tag|>", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0) content = content[..idx].TrimEnd();
        return string.IsNullOrWhiteSpace(content) ? "Sorry, I couldn't process that." : content;
    }
}
