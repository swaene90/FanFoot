using Fanfoot.Infrastructure.Services;

namespace Fanfoot.Web.Services;

public class PlayerImportService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PlayerImportService> _logger;

    public PlayerImportService(IServiceProvider services, ILogger<PlayerImportService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNextMidnightEst();
            _logger.LogInformation("Next player import at {Time} (in {Hours}h {Minutes}m)",
                DateTime.UtcNow.Add(delay), delay.Hours, delay.Minutes);

            await Task.Delay(delay, stoppingToken);

            await RunImportAsync(stoppingToken);
        }
    }

    private async Task RunImportAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _services.CreateScope();
            var leagueService = scope.ServiceProvider.GetRequiredService<LeagueService>();
            var count = await leagueService.ImportPlayersAsync(ct);
            _logger.LogInformation("Scheduled player import completed: {Count} players upserted", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled player import failed");
        }
    }

    private static TimeSpan TimeUntilNextMidnightEst()
    {
        var est = GetEstZone();
        var nowEst = TimeZoneInfo.ConvertTime(DateTime.UtcNow, est);
        var nextMidnight = nowEst.Date.AddDays(1);
        return nextMidnight - nowEst;
    }

    private static TimeZoneInfo GetEstZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/New_York"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); }
    }
}
