using System.Text.Json;
using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Clients;
using Fanfoot.Infrastructure.Data.Entities;

namespace Fanfoot.Infrastructure.Mapping;

public static class SleeperMapper
{
    public static PlayerEntity ToPlayer(SleeperPlayerDto dto)
    {
        return new PlayerEntity
        {
            Id = dto.PlayerId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Position = dto.Position,
            Team = dto.Team,
            Number = dto.Number,
            Age = dto.Age,
            Height = TryParseHeight(dto.Height),
            Weight = TryParseWeight(dto.Weight),
            College = dto.College,
            Status = dto.Status,
            InjuryStatus = dto.InjuryStatus,
            FantasyPositions = dto.FantasyPositions != null
                ? string.Join(",", dto.FantasyPositions)
                : null,
            YearsExp = dto.YearsExp,
            SearchFirstName = dto.SearchFirstName,
            SearchLastName = dto.SearchLastName,
            SearchFullName = dto.SearchFullName,
            Metadata = JsonSerializer.Serialize(new
            {
                dto.Hashtag,
                dto.DepthChartPosition,
                dto.DepthChartOrder,
                dto.SportradarId,
                dto.RotowireId,
                dto.RotoworldId,
                dto.YahooId,
                dto.EspnId,
                dto.StatsId,
                dto.FantasyDataId,
                dto.BirthCountry,
                dto.PracticeParticipation
            })
        };
    }

    public static LeagueEntity ToLeague(SleeperLeagueDto dto)
    {
        return new LeagueEntity
        {
            Id = dto.LeagueId,
            Name = dto.Name,
            Source = "sleeper",
            Avatar = dto.Avatar,
            Season = int.Parse(dto.Season),
            PreviousLeagueId = dto.PreviousLeagueId,
            DraftId = dto.DraftId,
            TotalRosters = dto.TotalRosters,
            Metadata = JsonSerializer.Serialize(new
            {
                dto.Status,
                dto.Sport,
                dto.SeasonType,
                dto.ScoringSettings,
                dto.RosterPositions
            }),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static TeamEntity ToTeam(SleeperRosterDto dto, string leagueId, Dictionary<string, string>? userTeamNames = null)
    {
        var settings = dto.Settings;
        var pointsFor = settings != null
            ? settings.Fpts + (settings.FptsDecimal / 100.0)
            : 0;
        var pointsAgainst = settings != null
            ? settings.FptsAgainst + (settings.FptsAgainstDecimal / 100.0)
            : 0;

        var teamName = dto.Metadata?.GetValueOrDefault("team_name");
        teamName ??= dto.OwnerId != null && userTeamNames?.TryGetValue(dto.OwnerId, out var userTeam) == true ? userTeam : null;
        teamName ??= $"Team {dto.RosterId}";

        return new TeamEntity
        {
            Id = $"{leagueId}_{dto.RosterId}",
            LeagueId = leagueId,
            OwnerId = dto.OwnerId,
            TeamName = teamName,
            Wins = settings?.Wins ?? 0,
            Losses = settings?.Losses ?? 0,
            Ties = settings?.Ties ?? 0,
            PointsFor = pointsFor,
            PointsAgainst = pointsAgainst,
            WaiverPosition = settings?.WaiverPosition,
            Starters = dto.Starters != null ? JsonSerializer.Serialize(dto.Starters) : null,
            Roster = dto.Players != null ? JsonSerializer.Serialize(dto.Players) : null,
            Reserve = dto.Reserve != null ? JsonSerializer.Serialize(dto.Reserve) : null,
            Taxi = dto.Taxi != null ? JsonSerializer.Serialize(dto.Taxi) : null,
            Settings = settings != null ? JsonSerializer.Serialize(settings) : null
        };
    }

    public static UserEntity ToUser(SleeperUserDto dto, string leagueId)
    {
        return new UserEntity
        {
            Id = dto.UserId,
            LeagueId = leagueId,
            DisplayName = dto.DisplayName ?? dto.Username,
            Avatar = dto.Avatar,
            IsOwner = dto.IsOwner ?? false,
            IsCommissioner = dto.IsCommissioner ?? false,
            Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null
        };
    }

    public static MatchupEntity ToMatchup(SleeperMatchupDto dto, string leagueId, int week, int season)
    {
        return new MatchupEntity
        {
            Id = $"{leagueId}_{week}_{dto.RosterId}",
            LeagueId = leagueId,
            Week = week,
            Season = season,
            TeamId = dto.RosterId.ToString(),
            Points = dto.CustomPoints ?? dto.Points,
            ProjectedPoints = dto.Points,
            MatchupId = dto.MatchupId,
            Starters = dto.Starters != null ? JsonSerializer.Serialize(dto.Starters) : null,
            StartersPoints = dto.StartersPoints != null ? JsonSerializer.Serialize(dto.StartersPoints) : null,
            Players = dto.Players != null ? JsonSerializer.Serialize(dto.Players) : null,
            PlayersPoints = dto.PlayersPoints != null ? JsonSerializer.Serialize(dto.PlayersPoints) : null
        };
    }

    public static DraftPickEntity ToDraftPick(SleeperDraftPickDto dto, string leagueId)
    {
        var meta = dto.Metadata;
        return new DraftPickEntity
        {
            Id = $"{dto.DraftId}_{dto.PickNo}",
            DraftId = dto.DraftId,
            LeagueId = leagueId,
            Round = dto.Round,
            PickNumber = dto.PickNo,
            OverallPickNumber = dto.PickNo,
            TeamId = dto.RosterId,
            PlayerId = dto.PlayerId,
            PlayerName = meta != null ? $"{meta.FirstName} {meta.LastName}".Trim() : null,
            Position = meta?.Position,
            Team = meta?.Team,
            IsKeeper = dto.IsKeeper,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static DraftInfo ToDraftInfo(SleeperDraftDto dto)
    {
        return new DraftInfo
        {
            DraftId = dto.DraftId,
            Type = dto.Type,
            Status = dto.Status,
            Season = dto.Season,
            DraftOrder = dto.DraftOrder,
            SlotToRosterId = dto.SlotToRosterId,
            Rounds = dto.Settings?.Rounds,
            Teams = dto.Settings?.Teams
        };
    }

    public static TradedPick ToTradedPick(SleeperTradedPickDto dto)
    {
        return new TradedPick
        {
            Season = dto.Season,
            Round = dto.Round,
            RosterId = dto.RosterId,
            OwnerId = dto.OwnerId,
            PreviousOwnerId = dto.PreviousOwnerId
        };
    }

    private static int? TryParseHeight(string? height)
    {
        if (string.IsNullOrEmpty(height)) return null;
        var parts = height.Split('\'');
        if (parts.Length == 2 && int.TryParse(parts[0], out var feet) && int.TryParse(parts[1].Replace("\"", "").Trim(), out var inches))
            return (feet * 12) + inches;
        return null;
    }

    private static int? TryParseWeight(string? weight)
    {
        if (string.IsNullOrEmpty(weight)) return null;
        if (int.TryParse(weight, out var result)) return result;
        return null;
    }
}
