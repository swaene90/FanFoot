using Fanfoot.Domain.Services;

namespace Fanfoot.Domain;

public static class ServiceExtensions
{
    public static IServiceCollection AddFanfootDomain(this IServiceCollection services)
    {
        services.AddScoped<LeagueService>();
        services.AddScoped<UserService>();
        services.AddScoped<AuthService>();
        services.AddScoped<PreferencesService>();
        services.AddScoped<ChatService>();
        services.AddScoped<ResourceAccessService>();
        services.AddHostedService<PlayerImportService>();

        return services;
    }
}
