using Microsoft.EntityFrameworkCore;
using Fanfoot.Infrastructure.Data.Entities;

namespace Fanfoot.Infrastructure.Data;

public class FanfootDbContext : DbContext
{
    public FanfootDbContext(DbContextOptions<FanfootDbContext> options) : base(options) { }

    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<LeagueEntity> Leagues => Set<LeagueEntity>();
    public DbSet<TeamEntity> Teams => Set<TeamEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<PlayerStatsEntity> PlayerStats => Set<PlayerStatsEntity>();
    public DbSet<MatchupEntity> Matchups => Set<MatchupEntity>();
    public DbSet<DraftPickEntity> DraftPicks => Set<DraftPickEntity>();
    public DbSet<LeagueSettingsEntity> LeagueSettings => Set<LeagueSettingsEntity>();
    public DbSet<LocalUserEntity> LocalUsers => Set<LocalUserEntity>();
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();
    public DbSet<ChatSessionEntity> ChatSessions => Set<ChatSessionEntity>();
    public DbSet<UserPreferencesEntity> UserPreferences => Set<UserPreferencesEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerEntity>(e =>
        {
            e.ToTable("Players");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasMaxLength(50);
            e.Property(p => p.FirstName).HasMaxLength(50);
            e.Property(p => p.LastName).HasMaxLength(50);
            e.Property(p => p.Position).HasMaxLength(20);
            e.Property(p => p.Team).HasMaxLength(20);
            e.Property(p => p.Status).HasMaxLength(100);
            e.Property(p => p.InjuryStatus).HasMaxLength(100);
            e.Property(p => p.College).HasMaxLength(200);
            e.Property(p => p.Metadata);
        });

        modelBuilder.Entity<LeagueEntity>(e =>
        {
            e.ToTable("Leagues");
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasMaxLength(50);
            e.Property(l => l.Name).HasMaxLength(200);
            e.Property(l => l.Source).HasMaxLength(20);
            e.Property(l => l.DraftId).HasMaxLength(50);
            e.Property(l => l.Metadata);
        });

        modelBuilder.Entity<TeamEntity>(e =>
        {
            e.ToTable("Teams");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasMaxLength(50);
            e.Property(t => t.LeagueId).HasMaxLength(50);
            e.Property(t => t.TeamName).HasMaxLength(200);
            e.Property(t => t.Avatar).HasMaxLength(500);
            e.HasIndex(t => t.LeagueId);
        });

        modelBuilder.Entity<UserEntity>(e =>
        {
            e.ToTable("Users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasMaxLength(50);
            e.Property(u => u.DisplayName).HasMaxLength(100);
            e.Property(u => u.RealName).HasMaxLength(200);
            e.Property(u => u.Metadata);
        });

        modelBuilder.Entity<PlayerStatsEntity>(e =>
        {
            e.ToTable("PlayerStats");
            e.HasKey(ps => ps.Id);
            e.Property(ps => ps.Id).HasMaxLength(100);
            e.Property(ps => ps.PlayerId).HasMaxLength(50);
            e.Property(ps => ps.LeagueId).HasMaxLength(50);
            e.HasIndex(ps => new { ps.PlayerId, ps.Season, ps.Week });
        });

        modelBuilder.Entity<MatchupEntity>(e =>
        {
            e.ToTable("Matchups");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasMaxLength(100);
            e.Property(m => m.LeagueId).HasMaxLength(50);
            e.Property(m => m.TeamId).HasMaxLength(50);
            e.HasIndex(m => new { m.LeagueId, m.Week, m.Season });
        });

        modelBuilder.Entity<DraftPickEntity>(e =>
        {
            e.ToTable("DraftPicks");
            e.HasKey(dp => dp.Id);
            e.Property(dp => dp.Id).HasMaxLength(100);
            e.Property(dp => dp.DraftId).HasMaxLength(50);
            e.Property(dp => dp.LeagueId).HasMaxLength(50);
            e.Property(dp => dp.PlayerName).HasMaxLength(200);
            e.Property(dp => dp.PlayerId).HasMaxLength(50);
            e.HasIndex(dp => dp.DraftId);
        });

        modelBuilder.Entity<LeagueSettingsEntity>(e =>
        {
            e.ToTable("LeagueSettings");
            e.HasKey(ls => ls.Id);
            e.Property(ls => ls.Id).HasMaxLength(50);
            e.Property(ls => ls.LeagueId).HasMaxLength(50);
        });

        modelBuilder.Entity<LocalUserEntity>(e =>
        {
            e.ToTable("LocalUsers");
            e.HasKey(u => u.SleeperUserId);
            e.Property(u => u.SleeperUserId).HasMaxLength(50);
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.PasswordHash).HasMaxLength(500);
            e.Property(u => u.SessionVersion).HasDefaultValue(1);
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<PasswordResetTokenEntity>(e =>
        {
            e.ToTable("PasswordResetTokens");
            e.HasKey(t => t.Id);
            e.Property(t => t.SleeperUserId).HasMaxLength(50);
            e.Property(t => t.TokenHash).HasMaxLength(64);
            e.HasIndex(t => t.TokenHash).IsUnique();
            e.HasIndex(t => new { t.SleeperUserId, t.ExpiresAt });
        });

        modelBuilder.Entity<ChatSessionEntity>(e =>
        {
            e.ToTable("ChatSessions");
            e.HasKey(cs => cs.Id);
            e.Property(cs => cs.Id).HasMaxLength(50);
            e.Property(cs => cs.UserId).HasMaxLength(50);
            e.Property(cs => cs.LeagueId).HasMaxLength(50);
            e.Property(cs => cs.Title).HasMaxLength(100);
            e.HasIndex(cs => new { cs.UserId, cs.UpdatedAt });
        });

        modelBuilder.Entity<UserPreferencesEntity>(e =>
        {
            e.ToTable("UserPreferences");
            e.HasKey(up => up.UserId);
            e.Property(up => up.UserId).HasMaxLength(50);
        });
    }
}
