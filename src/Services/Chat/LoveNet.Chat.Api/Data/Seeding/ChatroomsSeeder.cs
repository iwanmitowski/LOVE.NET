using System.Text.Json;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Chat.Api.Data.Seeding;

/// <summary>
/// Seeds the public chatrooms (monolith's Chatrooms.json) and announces each
/// via chat.room-provisioned so Realtime learns the public-room set.
/// </summary>
public static class ChatroomsSeeder
{
    public static async Task SeedAsync(ChatDbContext db)
    {
        if (await db.Chatrooms.AnyAsync())
        {
            return;
        }

        var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "Files", "Chatrooms.json");
        var chatrooms = JsonSerializer.Deserialize<Chatroom[]>(
            await File.ReadAllTextAsync(filePath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? Array.Empty<Chatroom>();

        foreach (var chatroom in chatrooms)
        {
            db.Chatrooms.Add(chatroom);

            db.AddOutboxMessage(
                KafkaTopics.RoomProvisioned,
                chatroom.Id,
                new RoomProvisionedEvent(chatroom.Id, IsPublic: true, Array.Empty<string>(), chatroom.Title));
        }

        await db.SaveChangesAsync();
    }
}
