using LoveNet.Chat.Api.Data;
using LoveNet.Chat.Api.Models;
using LoveNet.Chat.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Chat.Tests;

[TestFixture]
public class ChatServiceTests
{
    private const string RoomId = "room-1";

    private ChatDbContext db = null!;
    private PostgresMessageStore messageStore = null!;
    private ChatService chatService = null!;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new ChatDbContext(options);
        messageStore = new PostgresMessageStore(db);
        chatService = new ChatService(db, messageStore);

        db.UserProfiles.Add(new UserProfileLite
        {
            UserId = "sender-1",
            UserName = "alpha",
            AvatarUrl = "https://img/alpha.png",
        });

        for (var i = 0; i < 15; i++)
        {
            db.Messages.Add(new Message
            {
                Id = $"msg-{i}",
                RoomId = RoomId,
                SenderId = "sender-1",
                Text = $"message {i}",
                CreatedOn = DateTime.UtcNow.AddMinutes(i),
            });
        }

        await db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public async Task SuccessGetFirstPage()
    {
        var chat = await chatService.GetChatAsync(new ChatRequestModel { RoomId = RoomId, Page = 1 });

        Assert.Multiple(() =>
        {
            Assert.That(chat.TotalMessages, Is.EqualTo(15));
            Assert.That(chat.Messages.Count(), Is.EqualTo(10));
            // Newest first, with the sender's current avatar joined in.
            Assert.That(chat.Messages.First().Text, Is.EqualTo("message 14"));
            Assert.That(chat.Messages.First().ProfilePicture, Is.EqualTo("https://img/alpha.png"));
        });
    }

    [Test]
    public async Task SuccessGetSecondPage()
    {
        var chat = await chatService.GetChatAsync(new ChatRequestModel { RoomId = RoomId, Page = 2 });

        Assert.Multiple(() =>
        {
            Assert.That(chat.Messages.Count(), Is.EqualTo(5));
            Assert.That(chat.Messages.Last().Text, Is.EqualTo("message 0"));
        });
    }

    [Test]
    public async Task SuccessSaveMessageIsIdempotentById()
    {
        var message = new Message
        {
            Id = "dup-1",
            RoomId = RoomId,
            SenderId = "sender-1",
            Text = "hello",
            CreatedOn = DateTime.UtcNow,
        };

        await messageStore.AppendAsync(message);
        await messageStore.AppendAsync(new Message
        {
            Id = "dup-1",
            RoomId = RoomId,
            SenderId = "sender-1",
            Text = "hello again (redelivered)",
            CreatedOn = message.CreatedOn,
        });

        Assert.That(await db.Messages.CountAsync(m => m.Id == "dup-1"), Is.EqualTo(1));
    }

    [Test]
    public async Task CanAccessRoomCoversAllThreeGrants()
    {
        var userId = Guid.NewGuid().ToString();
        var otherId = Guid.NewGuid().ToString();
        var strangerRoom = string.Join(string.Empty, new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }.OrderBy(x => x));

        db.Chatrooms.Add(new Chatroom { Id = "public-room", Title = "Public", Url = "https://img/room.png" });
        db.Rooms.Add(new Room { RoomId = "provisioned-room", UserAId = userId, UserBId = otherId, CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var matchRoom = string.Join(string.Empty, new[] { userId, otherId }.OrderBy(x => x));

        Assert.That(await chatService.CanAccessRoomAsync("public-room", userId), Is.True, "public chatroom");
        Assert.That(await chatService.CanAccessRoomAsync("provisioned-room", userId), Is.True, "provisioned membership");
        Assert.That(await chatService.CanAccessRoomAsync(matchRoom, userId), Is.True, "deterministic roomId fallback");
        Assert.That(await chatService.CanAccessRoomAsync(strangerRoom, userId), Is.False, "someone else's room");
    }
}
