using LoveNet.BuildingBlocks.EventBus;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Chat.Api.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Chatroom> Chatrooms => Set<Chatroom>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<UserProfileLite> UserProfiles => Set<UserProfileLite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(m => new { m.Id, m.CreatedOn });
            entity.HasIndex(m => new { m.RoomId, m.CreatedOn }).IsDescending(false, true);
            entity.Property(m => m.Id).HasMaxLength(64);
            entity.Property(m => m.RoomId).HasMaxLength(128);
        });

        modelBuilder.Entity<Chatroom>(entity =>
        {
            entity.ToTable("chatrooms");
            entity.HasKey(c => c.Id);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("rooms");
            entity.HasKey(r => r.RoomId);
        });

        modelBuilder.Entity<UserProfileLite>(entity =>
        {
            entity.ToTable("user_profiles_lite");
            entity.HasKey(p => p.UserId);
        });

        modelBuilder.AddOutbox();
        modelBuilder.AddInbox();
    }
}
