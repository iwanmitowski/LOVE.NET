using LoveNet.BuildingBlocks.EventBus;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Matching.Api.Data;

public class MatchingDbContext : DbContext
{
    public MatchingDbContext(DbContextOptions<MatchingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Like> Likes => Set<Like>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("likes");
            entity.HasIndex(l => new { l.UserId, l.LikedUserId }).IsUnique();
            entity.HasIndex(l => l.LikedUserId);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("matches");
            entity.HasIndex(m => m.RoomId).IsUnique();
            entity.HasIndex(m => m.UserAId);
            entity.HasIndex(m => m.UserBId);
        });

        modelBuilder.Entity<CandidateProfile>(entity =>
        {
            entity.ToTable("candidate_profiles");
            entity.HasKey(p => p.UserId);
        });

        modelBuilder.AddOutbox();
        modelBuilder.AddInbox();
    }
}
