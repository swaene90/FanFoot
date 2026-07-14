using Fanfoot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fanfoot.Domain.Services;

public class ResourceAccessService(FanfootDbContext db)
{
    public Task<bool> IsLeagueMemberAsync(string userId, string leagueId) =>
        db.Teams.AnyAsync(team => team.LeagueId == leagueId && team.OwnerId == userId);

    public Task<bool> OwnsSessionAsync(string userId, string sessionId) =>
        db.ChatSessions.AnyAsync(session => session.Id == sessionId && session.UserId == userId);
}
