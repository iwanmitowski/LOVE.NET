using LoveNet.BuildingBlocks.EventBus;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Api.Data;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<Image> Images => Set<Image>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<Gender> Genders => Set<Gender>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(p => p.UserId);
            entity.HasMany(p => p.Images)
                .WithOne()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Country>()
            .HasMany(c => c.Cities)
            .WithOne()
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.AddOutbox();
        modelBuilder.AddInbox();
    }
}
