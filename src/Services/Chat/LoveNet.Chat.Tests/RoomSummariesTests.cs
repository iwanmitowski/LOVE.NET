using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.Chat.Api.Consumers;
using LoveNet.Chat.Api.Data;
using LoveNet.Chat.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace LoveNet.Chat.Tests;

[TestFixture]
public class RoomSummariesTests
{
    private static readonly string Me = Guid.NewGuid().ToString();
    private static readonly string Peer = Guid.NewGuid().ToString();

    private ChatDbContext db = null!;
    private ChatService chatService = null!;
    private string matchRoomId = null!;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new ChatDbContext(options);
        chatService = new ChatService(db, new PostgresMessageStore(db));

        matchRoomId = string.Join(string.Empty, new[] { Me, Peer }.OrderBy(x => x));

        db.Rooms.Add(new Room { RoomId = matchRoomId, UserAId = Me, UserBId = Peer, CreatedOn = DateTime.UtcNow });
        db.Chatrooms.Add(new Chatroom { Id = "public-1", Title = "Public", Url = "https://img/x.png" });

        var t0 = DateTime.UtcNow.AddMinutes(-10);
        db.Messages.AddRange(
            new Message { Id = "m1", RoomId = matchRoomId, SenderId = Peer, Text = "hey", CreatedOn = t0 },
            new Message { Id = "m2", RoomId = matchRoomId, SenderId = Me, Text = "hi!", CreatedOn = t0.AddMinutes(1) },
            new Message { Id = "m3", RoomId = matchRoomId, SenderId = Peer, Text = "how are you?", CreatedOn = t0.AddMinutes(2) },
            new Message { Id = "m4", RoomId = matchRoomId, SenderId = Peer, Text = "you there?", CreatedOn = t0.AddMinutes(3) });

        // I read up to (and including) m1 only; m3+m4 from the peer are unread.
        db.RoomReads.Add(new RoomRead { RoomId = matchRoomId, UserId = Me, LastReadAt = t0.AddSeconds(30) });
        db.RoomReads.Add(new RoomRead { RoomId = matchRoomId, UserId = Peer, LastReadAt = t0.AddMinutes(1.5) });

        await db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public async Task SummaryReportsLastMessageUnreadCountAndPeerRead()
    {
        var summaries = await chatService.GetSummariesAsync(new[] { matchRoomId }, Me);

        var summary = summaries.Single();
        Assert.Multiple(() =>
        {
            Assert.That(summary.LastMessageText, Is.EqualTo("you there?"));
            Assert.That(summary.LastMessageSenderId, Is.EqualTo(Peer));
            Assert.That(summary.UnreadCount, Is.EqualTo(2), "m3 and m4 are after my watermark; my own m2 never counts");
            Assert.That(summary.PeerLastReadAt, Is.Not.Null);
        });
    }

    [Test]
    public async Task NoWatermarkCountsAllPeerMessagesAsUnread()
    {
        db.RoomReads.Remove(db.RoomReads.Single(r => r.UserId == Me));
        await db.SaveChangesAsync();

        var summaries = await chatService.GetSummariesAsync(new[] { matchRoomId }, Me);

        Assert.That(summaries.Single().UnreadCount, Is.EqualTo(3));
    }

    [Test]
    public async Task InaccessibleRoomsAreOmittedAndPublicRoomsHaveNoPeerRead()
    {
        var strangersRoom = string.Join(string.Empty, new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }.OrderBy(x => x));

        var summaries = await chatService.GetSummariesAsync(new[] { matchRoomId, strangersRoom, "public-1" }, Me);

        Assert.Multiple(() =>
        {
            Assert.That(summaries.Select(s => s.RoomId), Is.EquivalentTo(new[] { matchRoomId, "public-1" }));
            Assert.That(summaries.Single(s => s.RoomId == "public-1").PeerLastReadAt, Is.Null);
        });
    }

    [Test]
    public async Task RoomReadConsumerWatermarkOnlyMovesForward()
    {
        var services = new ServiceCollection().AddSingleton(db).BuildServiceProvider();
        var consumer = new TestableRoomReadConsumer(
            services.GetRequiredService<IServiceScopeFactory>(),
            Mock.Of<IProducer<string, string>>(),
            Options.Create(new KafkaOptions()),
            NullLogger<RoomReadConsumer>.Instance);

        var newer = DateTime.UtcNow;
        var older = newer.AddMinutes(-30);

        await consumer.Handle(services, Envelope(new RoomReadEvent(matchRoomId, Me, newer)), CancellationToken.None);
        await consumer.Handle(services, Envelope(new RoomReadEvent(matchRoomId, Me, older)), CancellationToken.None);

        var watermark = await db.RoomReads.SingleAsync(r => r.RoomId == matchRoomId && r.UserId == Me);
        Assert.That(watermark.LastReadAt, Is.EqualTo(newer), "an older redelivered event must not regress the watermark");

        var summaries = await chatService.GetSummariesAsync(new[] { matchRoomId }, Me);
        Assert.That(summaries.Single().UnreadCount, Is.Zero);
    }

    private static EventEnvelope<RoomReadEvent> Envelope(RoomReadEvent data) =>
        EventEnvelope<RoomReadEvent>.Create(data);

    private sealed class TestableRoomReadConsumer : RoomReadConsumer
    {
        public TestableRoomReadConsumer(
            IServiceScopeFactory scopeFactory,
            IProducer<string, string> producer,
            IOptions<KafkaOptions> options,
            ILogger<RoomReadConsumer> logger)
            : base(scopeFactory, producer, options, logger)
        {
        }

        public Task Handle(IServiceProvider services, EventEnvelope<RoomReadEvent> envelope, CancellationToken ct) =>
            HandleAsync(services, envelope, ct);
    }
}
