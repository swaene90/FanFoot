using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Data.Entities;

namespace Fanfoot.Infrastructure.Mapping;

public static class EntityMapper
{
    public static League ToDomain(LeagueEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Source = e.Source,
        Avatar = e.Avatar,
        Season = e.Season,
        PreviousLeagueId = e.PreviousLeagueId,
        DraftId = e.DraftId,
        ScoringType = e.ScoringType,
        RosterPositions = e.RosterPositions,
        TotalRosters = e.TotalRosters,
        LastReadId = e.LastReadId,
        Metadata = e.Metadata,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    public static Team ToDomain(TeamEntity e) => new()
    {
        Id = e.Id,
        LeagueId = e.LeagueId,
        UserId = e.UserId,
        OwnerId = e.OwnerId,
        TeamName = e.TeamName,
        Avatar = e.Avatar,
        Wins = e.Wins,
        Losses = e.Losses,
        Ties = e.Ties,
        PointsFor = e.PointsFor,
        PointsAgainst = e.PointsAgainst,
        Standing = e.Standing,
        WaiverPriority = e.WaiverPriority,
        FaabBudget = e.FaabBudget,
        Settings = e.Settings,
        Roster = e.Roster,
        Starters = e.Starters,
        Taxi = e.Taxi,
        Practice = e.Practice,
        Reserve = e.Reserve,
        WaiverPosition = e.WaiverPosition,
        TradeBank = e.TradeBank
    };

    public static Player ToDomain(PlayerEntity e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Position = e.Position,
        Team = e.Team,
        Number = e.Number,
        Age = e.Age,
        Height = e.Height,
        Weight = e.Weight,
        College = e.College,
        Status = e.Status,
        InjuryStatus = e.InjuryStatus,
        FantasyPositions = e.FantasyPositions,
        YearsExp = e.YearsExp,
        SearchFirstName = e.SearchFirstName,
        SearchLastName = e.SearchLastName,
        SearchFullName = e.SearchFullName,
        Metadata = e.Metadata
    };

    public static User ToDomain(UserEntity e) => new()
    {
        Id = e.Id,
        LeagueId = e.LeagueId,
        DisplayName = e.DisplayName,
        Avatar = e.Avatar,
        RealName = e.RealName,
        Email = e.Email,
        IsOwner = e.IsOwner,
        IsCommissioner = e.IsCommissioner,
        UserMessage = e.UserMessage,
        Metadata = e.Metadata
    };

    public static LocalUser ToDomain(LocalUserEntity e) => new()
    {
        SleeperUserId = e.SleeperUserId,
        Email = e.Email,
        PasswordHash = e.PasswordHash,
        CreatedAt = e.CreatedAt,
        SessionVersion = e.SessionVersion
    };

    public static DraftPick ToDomain(DraftPickEntity e) => new()
    {
        Id = e.Id,
        DraftId = e.DraftId,
        LeagueId = e.LeagueId,
        Round = e.Round,
        PickNumber = e.PickNumber,
        OverallPickNumber = e.OverallPickNumber,
        TeamId = e.TeamId,
        PlayerId = e.PlayerId,
        PlayerName = e.PlayerName,
        Position = e.Position,
        Team = e.Team,
        IsKeeper = e.IsKeeper,
        CreatedAt = e.CreatedAt
    };

    public static ChatSession ToDomain(ChatSessionEntity e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        LeagueId = e.LeagueId,
        Title = e.Title,
        MessagesJson = e.MessagesJson,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    public static UserPreferences ToDomain(UserPreferencesEntity e) => new()
    {
        UserId = e.UserId,
        IsDarkMode = e.IsDarkMode,
        UpdatedAt = e.UpdatedAt
    };
}
