using Fanfoot.Domain.Models;
using Fanfoot.Infrastructure.Data;
using Fanfoot.Infrastructure.Data.Entities;
using Fanfoot.Infrastructure.Mapping;

namespace Fanfoot.Domain.Services;

public class PreferencesService
{
    private readonly FanfootDbContext _db;

    public PreferencesService(FanfootDbContext db) => _db = db;

    public async Task<UserPreferences?> GetAsync(string userId)
    {
        var prefs = await _db.UserPreferences.FindAsync(userId);
        return prefs == null ? null : EntityMapper.ToDomain(prefs);
    }

    public async Task SetDarkModeAsync(string userId, bool isDarkMode)
    {
        var prefs = await _db.UserPreferences.FindAsync(userId);
        if (prefs == null)
        {
            prefs = new UserPreferencesEntity { UserId = userId };
            _db.UserPreferences.Add(prefs);
        }
        prefs.IsDarkMode = isDarkMode;
        prefs.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
