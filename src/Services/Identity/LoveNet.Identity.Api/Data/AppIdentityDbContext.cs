using LoveNet.BuildingBlocks.EventBus;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Identity.Api.Data;

public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<UserProfileLite> UserProfiles => Set<UserProfileLite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserProfileLite>(entity =>
        {
            entity.ToTable("user_profiles_lite");
            entity.HasKey(p => p.UserId);
        });

        builder.AddOutbox();
        builder.AddInbox();
    }
}
