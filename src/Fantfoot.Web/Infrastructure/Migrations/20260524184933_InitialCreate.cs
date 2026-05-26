using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fantfoot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftPicks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DraftId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Round = table.Column<int>(type: "INTEGER", nullable: false),
                    PickNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    OverallPickNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    TeamId = table.Column<string>(type: "TEXT", nullable: true),
                    PlayerId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PlayerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Position = table.Column<string>(type: "TEXT", nullable: true),
                    Team = table.Column<string>(type: "TEXT", nullable: true),
                    IsKeeper = table.Column<bool>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPicks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Avatar = table.Column<string>(type: "TEXT", nullable: true),
                    Season = table.Column<int>(type: "INTEGER", nullable: false),
                    PreviousLeagueId = table.Column<string>(type: "TEXT", nullable: true),
                    DraftId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ScoringType = table.Column<int>(type: "INTEGER", nullable: false),
                    RosterPositions = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRosters = table.Column<int>(type: "INTEGER", nullable: false),
                    LastReadId = table.Column<int>(type: "INTEGER", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeagueSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    WaiverHours = table.Column<int>(type: "INTEGER", nullable: false),
                    WaiverType = table.Column<int>(type: "INTEGER", nullable: false),
                    TradeReviewDays = table.Column<int>(type: "INTEGER", nullable: false),
                    TradeDeadline = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayoffTeams = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayoffWeekStart = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayoffRoundType = table.Column<int>(type: "INTEGER", nullable: false),
                    BestBall = table.Column<int>(type: "INTEGER", nullable: true),
                    BenchLock = table.Column<int>(type: "INTEGER", nullable: true),
                    DailyWaivers = table.Column<int>(type: "INTEGER", nullable: true),
                    DailyWaiversDays = table.Column<int>(type: "INTEGER", nullable: true),
                    DailyWaiversHour = table.Column<int>(type: "INTEGER", nullable: true),
                    DisableAdds = table.Column<int>(type: "INTEGER", nullable: true),
                    DisableTrades = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftRound = table.Column<int>(type: "INTEGER", nullable: true),
                    LastReport = table.Column<int>(type: "INTEGER", nullable: true),
                    LockedPlayers = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxKeepers = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxMovements = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxTrades = table.Column<int>(type: "INTEGER", nullable: true),
                    MinBid = table.Column<int>(type: "INTEGER", nullable: true),
                    NumCommmisioners = table.Column<int>(type: "INTEGER", nullable: true),
                    OffseasonAdds = table.Column<int>(type: "INTEGER", nullable: true),
                    PickDroppable = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerLimit = table.Column<int>(type: "INTEGER", nullable: true),
                    ReserveAllowDnr = table.Column<int>(type: "INTEGER", nullable: true),
                    ReserveAllowSus = table.Column<int>(type: "INTEGER", nullable: true),
                    ReserveAllowOut = table.Column<int>(type: "INTEGER", nullable: true),
                    ReserveAllowDoubtful = table.Column<int>(type: "INTEGER", nullable: true),
                    ReserveAllowCow = table.Column<int>(type: "INTEGER", nullable: true),
                    ReserveSlots = table.Column<int>(type: "INTEGER", nullable: true),
                    StartWeek = table.Column<int>(type: "INTEGER", nullable: true),
                    TaxiAllowDnr = table.Column<int>(type: "INTEGER", nullable: true),
                    TaxiAllowSus = table.Column<int>(type: "INTEGER", nullable: true),
                    TaxiAllowVet = table.Column<int>(type: "INTEGER", nullable: true),
                    TaxiDeadline = table.Column<int>(type: "INTEGER", nullable: true),
                    TaxiSlots = table.Column<int>(type: "INTEGER", nullable: true),
                    TaxiYears = table.Column<int>(type: "INTEGER", nullable: true),
                    TradeBankSlots = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: true),
                    UniversalTicketCount = table.Column<int>(type: "INTEGER", nullable: true),
                    VetoShowVotes = table.Column<int>(type: "INTEGER", nullable: true),
                    VetoType = table.Column<int>(type: "INTEGER", nullable: true),
                    WaiverBudget = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matchups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Week = table.Column<int>(type: "INTEGER", nullable: false),
                    Season = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OpponentTeamId = table.Column<string>(type: "TEXT", nullable: true),
                    Points = table.Column<double>(type: "REAL", nullable: false),
                    ProjectedPoints = table.Column<double>(type: "REAL", nullable: false),
                    OpponentPoints = table.Column<double>(type: "REAL", nullable: true),
                    Starters = table.Column<string>(type: "TEXT", nullable: true),
                    StartersPoints = table.Column<string>(type: "TEXT", nullable: true),
                    Players = table.Column<string>(type: "TEXT", nullable: true),
                    PlayersPoints = table.Column<string>(type: "TEXT", nullable: true),
                    MatchupId = table.Column<int>(type: "INTEGER", nullable: true),
                    Won = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Team = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Number = table.Column<int>(type: "INTEGER", nullable: true),
                    Age = table.Column<int>(type: "INTEGER", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    Weight = table.Column<int>(type: "INTEGER", nullable: true),
                    College = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    InjuryStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    FantasyPositions = table.Column<string>(type: "TEXT", nullable: true),
                    YearsExp = table.Column<int>(type: "INTEGER", nullable: true),
                    SearchFirstName = table.Column<string>(type: "TEXT", nullable: true),
                    SearchLastName = table.Column<string>(type: "TEXT", nullable: true),
                    SearchFullName = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PlayerId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Season = table.Column<int>(type: "INTEGER", nullable: false),
                    Week = table.Column<int>(type: "INTEGER", nullable: false),
                    Points = table.Column<double>(type: "REAL", nullable: false),
                    ProjectedPoints = table.Column<double>(type: "REAL", nullable: false),
                    Stats = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: true),
                    TeamName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Avatar = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false),
                    Ties = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsFor = table.Column<double>(type: "REAL", nullable: false),
                    PointsAgainst = table.Column<double>(type: "REAL", nullable: false),
                    Standing = table.Column<int>(type: "INTEGER", nullable: true),
                    WaiverPriority = table.Column<int>(type: "INTEGER", nullable: true),
                    FaabBudget = table.Column<int>(type: "INTEGER", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true),
                    Roster = table.Column<string>(type: "TEXT", nullable: true),
                    Starters = table.Column<string>(type: "TEXT", nullable: true),
                    Taxi = table.Column<string>(type: "TEXT", nullable: true),
                    Practice = table.Column<string>(type: "TEXT", nullable: true),
                    Reserve = table.Column<string>(type: "TEXT", nullable: true),
                    WaiverPosition = table.Column<int>(type: "INTEGER", nullable: true),
                    TradeBank = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Avatar = table.Column<string>(type: "TEXT", nullable: true),
                    RealName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    IsOwner = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsCommissioner = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserMessage = table.Column<int>(type: "INTEGER", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftPicks_DraftId",
                table: "DraftPicks",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_Matchups_LeagueId_Week_Season",
                table: "Matchups",
                columns: new[] { "LeagueId", "Week", "Season" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerId_Season_Week",
                table: "PlayerStats",
                columns: new[] { "PlayerId", "Season", "Week" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LeagueId",
                table: "Teams",
                column: "LeagueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftPicks");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "LeagueSettings");

            migrationBuilder.DropTable(
                name: "Matchups");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "PlayerStats");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
