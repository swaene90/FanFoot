using Microsoft.EntityFrameworkCore;
using Fantfoot.Domain;

namespace Fantfoot.Infrastructure.Data;

public class FantfootDbContext : DbContext
{
    public FantfootDbContext(DbContextOptions<FantfootDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PlayerStats> PlayerStats => Set<PlayerStats>();
    public DbSet<Matchup> Matchups => Set<Matchup>();
    public DbSet<DraftPick> DraftPicks => Set<DraftPick>();
    public DbSet<LeagueSettings> LeagueSettings => Set<LeagueSettings>();
    public DbSet<LocalUser> LocalUsers => Set<LocalUser>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(e =>
        {
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

        modelBuilder.Entity<League>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasMaxLength(50);
            e.Property(l => l.Name).HasMaxLength(200);
            e.Property(l => l.Source).HasMaxLength(20);
            e.Property(l => l.DraftId).HasMaxLength(50);
            e.Property(l => l.Metadata);
        });

        modelBuilder.Entity<Team>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasMaxLength(50);
            e.Property(t => t.LeagueId).HasMaxLength(50);
            e.Property(t => t.TeamName).HasMaxLength(200);
            e.Property(t => t.Avatar).HasMaxLength(500);
            e.HasIndex(t => t.LeagueId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasMaxLength(50);
            e.Property(u => u.DisplayName).HasMaxLength(100);
            e.Property(u => u.RealName).HasMaxLength(200);
            e.Property(u => u.Metadata);
        });

        modelBuilder.Entity<PlayerStats>(e =>
        {
            e.HasKey(ps => ps.Id);
            e.Property(ps => ps.Id).HasMaxLength(100);
            e.Property(ps => ps.PlayerId).HasMaxLength(50);
            e.Property(ps => ps.LeagueId).HasMaxLength(50);
            e.HasIndex(ps => new { ps.PlayerId, ps.Season, ps.Week });
        });

        modelBuilder.Entity<Matchup>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasMaxLength(100);
            e.Property(m => m.LeagueId).HasMaxLength(50);
            e.Property(m => m.TeamId).HasMaxLength(50);
            e.HasIndex(m => new { m.LeagueId, m.Week, m.Season });
        });

        modelBuilder.Entity<DraftPick>(e =>
        {
            e.HasKey(dp => dp.Id);
            e.Property(dp => dp.Id).HasMaxLength(100);
            e.Property(dp => dp.DraftId).HasMaxLength(50);
            e.Property(dp => dp.LeagueId).HasMaxLength(50);
            e.Property(dp => dp.PlayerName).HasMaxLength(200);
            e.Property(dp => dp.PlayerId).HasMaxLength(50);
            e.HasIndex(dp => dp.DraftId);
        });

        modelBuilder.Entity<LeagueSettings>(e =>
        {
            e.HasKey(ls => ls.Id);
            e.Property(ls => ls.Id).HasMaxLength(50);
            e.Property(ls => ls.LeagueId).HasMaxLength(50);
        });

        modelBuilder.Entity<LocalUser>(e =>
        {
            e.HasKey(u => u.SleeperUserId);
            e.Property(u => u.SleeperUserId).HasMaxLength(50);
        });

        modelBuilder.Entity<ChatSession>(e =>
        {
            e.HasKey(cs => cs.Id);
            e.Property(cs => cs.Id).HasMaxLength(50);
            e.Property(cs => cs.UserId).HasMaxLength(50);
            e.Property(cs => cs.LeagueId).HasMaxLength(50);
            e.Property(cs => cs.Title).HasMaxLength(100);
            e.HasIndex(cs => new { cs.UserId, cs.UpdatedAt });
        });
    }
}
