using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Fantfoot.Infrastructure.Clients;
using Fantfoot.Infrastructure.Data;
using Fantfoot.Infrastructure.Services;

namespace Fantfoot.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddFantfootInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<FantfootDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpClient<SleeperClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.sleeper.app/v1/");
            client.DefaultRequestHeaders.Add("User-Agent", "Fantfoot/1.0");
        });

        services.AddScoped<LeagueService>();

        return services;
    }
}
