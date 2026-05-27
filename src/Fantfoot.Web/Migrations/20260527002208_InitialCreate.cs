using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fantfoot.Web.Migrations
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
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DraftId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    PickNumber = table.Column<int>(type: "integer", nullable: false),
                    OverallPickNumber = table.Column<int>(type: "integer", nullable: true),
                    TeamId = table.Column<string>(type: "text", nullable: true),
                    PlayerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PlayerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    Team = table.Column<string>(type: "text", nullable: true),
                    IsKeeper = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPicks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Season = table.Column<int>(type: "integer", nullable: false),
                    PreviousLeagueId = table.Column<string>(type: "text", nullable: true),
                    DraftId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ScoringType = table.Column<int>(type: "integer", nullable: false),
                    RosterPositions = table.Column<int>(type: "integer", nullable: false),
                    TotalRosters = table.Column<int>(type: "integer", nullable: false),
                    LastReadId = table.Column<int>(type: "integer", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeagueSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    WaiverHours = table.Column<int>(type: "integer", nullable: false),
                    WaiverType = table.Column<int>(type: "integer", nullable: false),
                    TradeReviewDays = table.Column<int>(type: "integer", nullable: false),
                    TradeDeadline = table.Column<int>(type: "integer", nullable: false),
                    PlayoffTeams = table.Column<int>(type: "integer", nullable: false),
                    PlayoffWeekStart = table.Column<int>(type: "integer", nullable: false),
                    PlayoffRoundType = table.Column<int>(type: "integer", nullable: false),
                    BestBall = table.Column<int>(type: "integer", nullable: true),
                    BenchLock = table.Column<int>(type: "integer", nullable: true),
                    DailyWaivers = table.Column<int>(type: "integer", nullable: true),
                    DailyWaiversDays = table.Column<int>(type: "integer", nullable: true),
                    DailyWaiversHour = table.Column<int>(type: "integer", nullable: true),
                    DisableAdds = table.Column<int>(type: "integer", nullable: true),
                    DisableTrades = table.Column<int>(type: "integer", nullable: true),
                    DraftRound = table.Column<int>(type: "integer", nullable: true),
                    LastReport = table.Column<int>(type: "integer", nullable: true),
                    LockedPlayers = table.Column<int>(type: "integer", nullable: true),
                    MaxKeepers = table.Column<int>(type: "integer", nullable: true),
                    MaxMovements = table.Column<int>(type: "integer", nullable: true),
                    MaxTrades = table.Column<int>(type: "integer", nullable: true),
                    MinBid = table.Column<int>(type: "integer", nullable: true),
                    NumCommmisioners = table.Column<int>(type: "integer", nullable: true),
                    OffseasonAdds = table.Column<int>(type: "integer", nullable: true),
                    PickDroppable = table.Column<int>(type: "integer", nullable: true),
                    PlayerLimit = table.Column<int>(type: "integer", nullable: true),
                    ReserveAllowDnr = table.Column<int>(type: "integer", nullable: true),
                    ReserveAllowSus = table.Column<int>(type: "integer", nullable: true),
                    ReserveAllowOut = table.Column<int>(type: "integer", nullable: true),
                    ReserveAllowDoubtful = table.Column<int>(type: "integer", nullable: true),
                    ReserveAllowCow = table.Column<int>(type: "integer", nullable: true),
                    ReserveSlots = table.Column<int>(type: "integer", nullable: true),
                    StartWeek = table.Column<int>(type: "integer", nullable: true),
                    TaxiAllowDnr = table.Column<int>(type: "integer", nullable: true),
                    TaxiAllowSus = table.Column<int>(type: "integer", nullable: true),
                    TaxiAllowVet = table.Column<int>(type: "integer", nullable: true),
                    TaxiDeadline = table.Column<int>(type: "integer", nullable: true),
                    TaxiSlots = table.Column<int>(type: "integer", nullable: true),
                    TaxiYears = table.Column<int>(type: "integer", nullable: true),
                    TradeBankSlots = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    UniversalTicketCount = table.Column<int>(type: "integer", nullable: true),
                    VetoShowVotes = table.Column<int>(type: "integer", nullable: true),
                    VetoType = table.Column<int>(type: "integer", nullable: true),
                    WaiverBudget = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalUsers",
                columns: table => new
                {
                    SleeperUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalUsers", x => x.SleeperUserId);
                });

            migrationBuilder.CreateTable(
                name: "Matchups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Week = table.Column<int>(type: "integer", nullable: false),
                    Season = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OpponentTeamId = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<double>(type: "double precision", nullable: false),
                    ProjectedPoints = table.Column<double>(type: "double precision", nullable: false),
                    OpponentPoints = table.Column<double>(type: "double precision", nullable: true),
                    Starters = table.Column<string>(type: "text", nullable: true),
                    StartersPoints = table.Column<string>(type: "text", nullable: true),
                    Players = table.Column<string>(type: "text", nullable: true),
                    PlayersPoints = table.Column<string>(type: "text", nullable: true),
                    MatchupId = table.Column<int>(type: "integer", nullable: true),
                    Won = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Team = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: true),
                    College = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InjuryStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FantasyPositions = table.Column<string>(type: "text", nullable: true),
                    YearsExp = table.Column<int>(type: "integer", nullable: true),
                    SearchFirstName = table.Column<string>(type: "text", nullable: true),
                    SearchLastName = table.Column<string>(type: "text", nullable: true),
                    SearchFullName = table.Column<string>(type: "text", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlayerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Season = table.Column<int>(type: "integer", nullable: false),
                    Week = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<double>(type: "double precision", nullable: false),
                    ProjectedPoints = table.Column<double>(type: "double precision", nullable: false),
                    Stats = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    TeamName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Avatar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    Ties = table.Column<int>(type: "integer", nullable: false),
                    PointsFor = table.Column<double>(type: "double precision", nullable: false),
                    PointsAgainst = table.Column<double>(type: "double precision", nullable: false),
                    Standing = table.Column<int>(type: "integer", nullable: true),
                    WaiverPriority = table.Column<int>(type: "integer", nullable: true),
                    FaabBudget = table.Column<int>(type: "integer", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true),
                    Roster = table.Column<string>(type: "text", nullable: true),
                    Starters = table.Column<string>(type: "text", nullable: true),
                    Taxi = table.Column<string>(type: "text", nullable: true),
                    Practice = table.Column<string>(type: "text", nullable: true),
                    Reserve = table.Column<string>(type: "text", nullable: true),
                    WaiverPosition = table.Column<int>(type: "integer", nullable: true),
                    TradeBank = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LeagueId = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    RealName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    IsOwner = table.Column<bool>(type: "boolean", nullable: false),
                    IsCommissioner = table.Column<bool>(type: "boolean", nullable: false),
                    UserMessage = table.Column<int>(type: "integer", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true)
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
                name: "LocalUsers");

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
