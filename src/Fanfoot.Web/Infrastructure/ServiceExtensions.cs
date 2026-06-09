using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Fanfoot.Infrastructure.Clients;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Services;

namespace Fanfoot.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddFanfootInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<FanfootDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpClient<SleeperClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.sleeper.app/v1/");
            client.DefaultRequestHeaders.Add("User-Agent", "Fanfoot/1.0");
        });

        services.AddHttpClient<FantasyCalcClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.fantasycalc.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "Fanfoot/1.0");
        });

        services.AddHttpClient("ESPN", client =>
        {
            client.BaseAddress = new Uri("https://site.api.espn.com/");
        });

        services.AddScoped<LeagueService>();

        return services;
    }
}
