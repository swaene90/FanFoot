namespace Fanfoot.Infrastructure.Data.Entities;

public class LeagueSettingsEntity
{
    public string Id { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int WaiverHours { get; set; }
    public int WaiverType { get; set; }
    public int TradeReviewDays { get; set; }
    public int TradeDeadline { get; set; }
    public int PlayoffTeams { get; set; }
    public int PlayoffWeekStart { get; set; }
    public int PlayoffRoundType { get; set; }
    public int? BestBall { get; set; }
    public int? BenchLock { get; set; }
    public int? DailyWaivers { get; set; }
    public int? DailyWaiversDays { get; set; }
    public int? DailyWaiversHour { get; set; }
    public int? DisableAdds { get; set; }
    public int? DisableTrades { get; set; }
    public int? DraftRound { get; set; }
    public int? LastReport { get; set; }
    public int? LockedPlayers { get; set; }
    public int? MaxKeepers { get; set; }
    public int? MaxMovements { get; set; }
    public int? MaxTrades { get; set; }
    public int? MinBid { get; set; }
    public int? NumCommmisioners { get; set; }
    public int? OffseasonAdds { get; set; }
    public int? PickDroppable { get; set; }
    public int? PlayerLimit { get; set; }
    public int? ReserveAllowDnr { get; set; }
    public int? ReserveAllowSus { get; set; }
    public int? ReserveAllowOut { get; set; }
    public int? ReserveAllowDoubtful { get; set; }
    public int? ReserveAllowCow { get; set; }
    public int? ReserveSlots { get; set; }
    public int? StartWeek { get; set; }
    public int? TaxiAllowDnr { get; set; }
    public int? TaxiAllowSus { get; set; }
    public int? TaxiAllowVet { get; set; }
    public int? TaxiDeadline { get; set; }
    public int? TaxiSlots { get; set; }
    public int? TaxiYears { get; set; }
    public int? TradeBankSlots { get; set; }
    public int? Type { get; set; }
    public int? UniversalTicketCount { get; set; }
    public int? VetoShowVotes { get; set; }
    public int? VetoType { get; set; }
    public string? WaiverBudget { get; set; }
}
