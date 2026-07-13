using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.Notifications.Api.Data;
using LoveNet.Notifications.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LoveNet.Notifications.Tests;

[TestFixture]
public class EmailDispatchServiceTests
{
    private NotificationsDbContext db = null!;
    private Mock<IEmailSender> emailSender = null!;
    private EmailDispatchService service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new NotificationsDbContext(options);
        emailSender = new Mock<IEmailSender>();

        var templateProvider = new Mock<IEmailTemplateProvider>();
        templateProvider
            .Setup(p => p.GetTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("link: {0}");

        var configuration = new ConfigurationBuilder().Build();
        service = new EmailDispatchService(db, emailSender.Object, templateProvider.Object, configuration);
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public async Task HandleEmailRequestedAsync_SendsEmailWithLinkSubstitutedAndWritesLog()
    {
        var evt = new EmailRequestedEvent(
            EmailType.ConfirmEmail,
            "user@example.com",
            "https://love.net/confirm?token=abc");

        await service.HandleEmailRequestedAsync(Guid.NewGuid(), evt, CancellationToken.None);

        emailSender.Verify(
            s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "user@example.com",
                "Verify Email",
                "link: https://love.net/confirm?token=abc",
                It.IsAny<IEnumerable<EmailAttachment>?>()),
            Times.Once);

        var log = await db.EmailLogs.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(log.ToEmail, Is.EqualTo("user@example.com"));
            Assert.That(log.Subject, Is.EqualTo("Verify Email"));
            Assert.That(log.EmailType, Is.EqualTo(nameof(EmailType.ConfirmEmail)));
        });
    }

    [Test]
    public async Task HandleEmailRequestedAsync_SameMessageIdTwice_SendsExactlyOnce()
    {
        var messageId = Guid.NewGuid();
        var evt = new EmailRequestedEvent(
            EmailType.ResetPassword,
            "user@example.com",
            "https://love.net/reset?token=xyz");

        await service.HandleEmailRequestedAsync(messageId, evt, CancellationToken.None);
        await service.HandleEmailRequestedAsync(messageId, evt, CancellationToken.None);

        emailSender.Verify(
            s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<EmailAttachment>?>()),
            Times.Once);

        Assert.That(await db.EmailLogs.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task HandleMatchCreatedAsync_SendsOneEmailToEachUser()
    {
        var userA = new MatchedUser("user-a", "Alice", "alice@example.com", null);
        var userB = new MatchedUser("user-b", "Bob", "bob@example.com", null);
        var evt = new MatchCreatedEvent("match-1", "room-1", userA, userB, DateTime.UtcNow);

        await service.HandleMatchCreatedAsync(Guid.NewGuid(), evt, CancellationToken.None);

        emailSender.Verify(
            s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "alice@example.com",
                "You have a new match",
                It.Is<string>(content => content.Contains("Bob")),
                It.IsAny<IEnumerable<EmailAttachment>?>()),
            Times.Once);

        emailSender.Verify(
            s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "bob@example.com",
                "You have a new match",
                It.Is<string>(content => content.Contains("Alice")),
                It.IsAny<IEnumerable<EmailAttachment>?>()),
            Times.Once);

        Assert.That(await db.EmailLogs.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task UpsertContactAsync_CreatesThenUpdatesContact()
    {
        await service.UpsertContactAsync(Guid.NewGuid(), "user-1", "old@example.com", "OldName", CancellationToken.None);

        var created = await db.UserContacts.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(created.UserId, Is.EqualTo("user-1"));
            Assert.That(created.Email, Is.EqualTo("old@example.com"));
            Assert.That(created.UserName, Is.EqualTo("OldName"));
        });

        await service.UpsertContactAsync(Guid.NewGuid(), "user-1", "new@example.com", "NewName", CancellationToken.None);

        var updated = await db.UserContacts.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updated.UserId, Is.EqualTo("user-1"));
            Assert.That(updated.Email, Is.EqualTo("new@example.com"));
            Assert.That(updated.UserName, Is.EqualTo("NewName"));
        });
    }
}
