using System.Text.Json.Serialization;

namespace Fanfoot.Infrastructure.Clients;

public class SleeperPlayerDto
{
    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("team")]
    public string? Team { get; set; }

    [JsonPropertyName("number")]
    public int? Number { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("height")]
    public string? Height { get; set; }

    [JsonPropertyName("weight")]
    public string? Weight { get; set; }

    [JsonPropertyName("college")]
    public string? College { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("injury_status")]
    public string? InjuryStatus { get; set; }

    [JsonPropertyName("fantasy_positions")]
    public List<string>? FantasyPositions { get; set; }

    [JsonPropertyName("years_exp")]
    public int? YearsExp { get; set; }

    [JsonPropertyName("search_first_name")]
    public string? SearchFirstName { get; set; }

    [JsonPropertyName("search_last_name")]
    public string? SearchLastName { get; set; }

    [JsonPropertyName("search_full_name")]
    public string? SearchFullName { get; set; }

    [JsonPropertyName("depth_chart_position")]
    public string? DepthChartPosition { get; set; }

    [JsonPropertyName("depth_chart_order")]
    public int? DepthChartOrder { get; set; }

    [JsonPropertyName("sportradar_id")]
    public string? SportradarId { get; set; }

    [JsonPropertyName("rotowire_id")]
    public int? RotowireId { get; set; }

    [JsonPropertyName("rotoworld_id")]
    public int? RotoworldId { get; set; }

    [JsonPropertyName("yahoo_id")]
    public int? YahooId { get; set; }

    [JsonPropertyName("espn_id")]
    public int? EspnId { get; set; }

    [JsonPropertyName("stats_id")]
    public int? StatsId { get; set; }

    [JsonPropertyName("fantasy_data_id")]
    public int? FantasyDataId { get; set; }

    [JsonPropertyName("hashtag")]
    public string? Hashtag { get; set; }

    [JsonPropertyName("birth_country")]
    public string? BirthCountry { get; set; }

    [JsonPropertyName("practice_participation")]
    public string? PracticeParticipation { get; set; }
}

public class SleeperLeagueDto
{
    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("season")]
    public string Season { get; set; } = string.Empty;

    [JsonPropertyName("season_type")]
    public string SeasonType { get; set; } = string.Empty;

    [JsonPropertyName("previous_league_id")]
    public string? PreviousLeagueId { get; set; }

    [JsonPropertyName("draft_id")]
    public string? DraftId { get; set; }

    [JsonPropertyName("total_rosters")]
    public int TotalRosters { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("sport")]
    public string Sport { get; set; } = string.Empty;

    [JsonPropertyName("scoring_settings")]
    public Dictionary<string, object>? ScoringSettings { get; set; }

    [JsonPropertyName("roster_positions")]
    public List<string>? RosterPositions { get; set; }

    [JsonPropertyName("settings")]
    public Dictionary<string, object>? Settings { get; set; }
}

public class SleeperRosterDto
{
    [JsonPropertyName("roster_id")]
    public int RosterId { get; set; }

    [JsonPropertyName("owner_id")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; } = string.Empty;

    [JsonPropertyName("starters")]
    public List<string>? Starters { get; set; }

    [JsonPropertyName("players")]
    public List<string>? Players { get; set; }

    [JsonPropertyName("reserve")]
    public List<string>? Reserve { get; set; }

    [JsonPropertyName("taxi")]
    public List<string>? Taxi { get; set; }

    [JsonPropertyName("settings")]
    public SleeperRosterSettingsDto? Settings { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}

public class SleeperRosterSettingsDto
{
    [JsonPropertyName("wins")]
    public int Wins { get; set; }

    [JsonPropertyName("losses")]
    public int Losses { get; set; }

    [JsonPropertyName("ties")]
    public int Ties { get; set; }

    [JsonPropertyName("fpts")]
    public int Fpts { get; set; }

    [JsonPropertyName("fpts_decimal")]
    public int FptsDecimal { get; set; }

    [JsonPropertyName("fpts_against")]
    public int FptsAgainst { get; set; }

    [JsonPropertyName("fpts_against_decimal")]
    public int FptsAgainstDecimal { get; set; }

    [JsonPropertyName("waiver_position")]
    public int? WaiverPosition { get; set; }

    [JsonPropertyName("waiver_budget_used")]
    public int? WaiverBudgetUsed { get; set; }

    [JsonPropertyName("total_moves")]
    public int? TotalMoves { get; set; }

    [JsonPropertyName("total_trades")]
    public int? TotalTrades { get; set; }
}

public class SleeperUserDto
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("is_owner")]
    public bool? IsOwner { get; set; }

    [JsonPropertyName("is_commissioner")]
    public bool? IsCommissioner { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}

public class SleeperMatchupDto
{
    [JsonPropertyName("roster_id")]
    public int RosterId { get; set; }

    [JsonPropertyName("matchup_id")]
    public int MatchupId { get; set; }

    [JsonPropertyName("points")]
    public double Points { get; set; }

    [JsonPropertyName("custom_points")]
    public double? CustomPoints { get; set; }

    [JsonPropertyName("starters")]
    public List<string>? Starters { get; set; }

    [JsonPropertyName("starters_points")]
    public List<double>? StartersPoints { get; set; }

    [JsonPropertyName("players")]
    public List<string>? Players { get; set; }

    [JsonPropertyName("players_points")]
    public Dictionary<string, double>? PlayersPoints { get; set; }
}

public class SleeperDraftPickDto
{
    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("picked_by")]
    public string? PickedBy { get; set; }

    [JsonPropertyName("roster_id")]
    public string? RosterId { get; set; }

    [JsonPropertyName("round")]
    public int Round { get; set; }

    [JsonPropertyName("pick_no")]
    public int PickNo { get; set; }

    [JsonPropertyName("draft_slot")]
    public int DraftSlot { get; set; }

    [JsonPropertyName("is_keeper")]
    public bool? IsKeeper { get; set; }

    [JsonPropertyName("draft_id")]
    public string DraftId { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public SleeperDraftPickMetadataDto? Metadata { get; set; }
}

public class SleeperDraftPickMetadataDto
{
    [JsonPropertyName("team")]
    public string? Team { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("sport")]
    public string? Sport { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("player_id")]
    public string? PlayerId { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("injury_status")]
    public string? InjuryStatus { get; set; }
}

public class SleeperDraftDto
{
    [JsonPropertyName("draft_id")]
    public string DraftId { get; set; } = string.Empty;

    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("season")]
    public string? Season { get; set; }

    [JsonPropertyName("season_type")]
    public string? SeasonType { get; set; }

    [JsonPropertyName("draft_order")]
    public Dictionary<string, int>? DraftOrder { get; set; }

    [JsonPropertyName("slot_to_roster_id")]
    public Dictionary<string, int>? SlotToRosterId { get; set; }

    [JsonPropertyName("settings")]
    public SleeperDraftSettingsDto? Settings { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("start_time")]
    public long? StartTime { get; set; }

    [JsonPropertyName("last_picked")]
    public long? LastPicked { get; set; }
}

public class SleeperTradedPickDto
{
    [JsonPropertyName("round")]
    public int Round { get; set; }

    [JsonPropertyName("season")]
    public string Season { get; set; } = string.Empty;

    [JsonPropertyName("roster_id")]
    public int RosterId { get; set; }

    [JsonPropertyName("owner_id")]
    public int OwnerId { get; set; }

    [JsonPropertyName("previous_owner_id")]
    public int PreviousOwnerId { get; set; }
}

public class SleeperDraftSettingsDto
{
    [JsonPropertyName("rounds")]
    public int? Rounds { get; set; }

    [JsonPropertyName("teams")]
    public int? Teams { get; set; }

    [JsonPropertyName("pick_timer")]
    public int? PickTimer { get; set; }

    [JsonPropertyName("nomination_timer")]
    public int? NominationTimer { get; set; }

    [JsonPropertyName("reversal_round")]
    public int? ReversalRound { get; set; }
}

public class SleeperNflStateDto
{
    [JsonPropertyName("week")]
    public int Week { get; set; }

    [JsonPropertyName("season")]
    public string Season { get; set; } = string.Empty;

    [JsonPropertyName("season_type")]
    public string SeasonType { get; set; } = string.Empty;

    [JsonPropertyName("leg")]
    public int? Leg { get; set; }

    [JsonPropertyName("season_start_date")]
    public string? SeasonStartDate { get; set; }

    [JsonPropertyName("display_week")]
    public int? DisplayWeek { get; set; }

    [JsonPropertyName("previous_season")]
    public string? PreviousSeason { get; set; }

    [JsonPropertyName("league_season")]
    public string? LeagueSeason { get; set; }

    [JsonPropertyName("league_create_season")]
    public string? LeagueCreateSeason { get; set; }
}
