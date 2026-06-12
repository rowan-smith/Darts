using DartsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DartsApi.Data;

public class DartsDbContext(DbContextOptions<DartsDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Set> Sets => Set<Set>();
    public DbSet<Leg> Legs => Set<Leg>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Suggestion> Suggestions => Set<Suggestion>();
    public DbSet<FeaturedHighlight> FeaturedHighlights => Set<FeaturedHighlight>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>()
            .HasOne(m => m.Player1)
            .WithMany(p => p.MatchesAsPlayer1)
            .HasForeignKey(m => m.Player1Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.Player2)
            .WithMany(p => p.MatchesAsPlayer2)
            .HasForeignKey(m => m.Player2Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.Winner)
            .WithMany(p => p.MatchesWon)
            .HasForeignKey(m => m.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Set>()
            .HasOne(s => s.Winner)
            .WithMany()
            .HasForeignKey(s => s.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Leg>()
            .HasOne(l => l.Winner)
            .WithMany(p => p.LegsWon)
            .HasForeignKey(l => l.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Visit>()
            .HasOne(v => v.Player)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeaturedHighlight>()
            .HasOne(f => f.Match)
            .WithMany()
            .HasForeignKey(f => f.MatchId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Match>().HasIndex(m => m.Status);
        modelBuilder.Entity<Article>().HasIndex(a => a.IsFeatured);
        modelBuilder.Entity<Suggestion>().HasIndex(s => s.IsActive);
    }
}
