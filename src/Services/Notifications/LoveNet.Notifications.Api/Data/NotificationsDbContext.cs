using LoveNet.BuildingBlocks.EventBus;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Notifications.Api.Data;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    public DbSet<UserContact> UserContacts => Set<UserContact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.ToTable("email_log");
            entity.HasKey(l => l.Id);
        });

        modelBuilder.Entity<UserContact>(entity =>
        {
            entity.ToTable("user_contacts");
            entity.HasKey(c => c.UserId);
        });

        modelBuilder.AddInbox();
    }
}
