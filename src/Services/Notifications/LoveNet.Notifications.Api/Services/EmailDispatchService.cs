using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.Notifications.Api.Data;

namespace LoveNet.Notifications.Api.Services;

/// <summary>
/// Testable core the Kafka consumers delegate to. Every handler is idempotent
/// via the inbox: the inbox row commits in the same SaveChanges as the email
/// log, after the send succeeded.
/// </summary>
public class EmailDispatchService : IEmailDispatchService
{
    private const string ConsumerName = "notifications-service";
    private const string FromEmail = "parentassistantapi@abv.bg";
    private const string FromName = "ParentAssistantApi";

    private readonly NotificationsDbContext db;
    private readonly IEmailSender emailSender;
    private readonly IEmailTemplateProvider templateProvider;

    public EmailDispatchService(
        NotificationsDbContext db,
        IEmailSender emailSender,
        IEmailTemplateProvider templateProvider)
    {
        this.db = db;
        this.emailSender = emailSender;
        this.templateProvider = templateProvider;
    }

    public async Task HandleEmailRequestedAsync(Guid messageId, EmailRequestedEvent evt, CancellationToken ct)
    {
        if (!await db.TryRegisterInboxAsync(messageId, ConsumerName, ct))
        {
            return;
        }

        var (templateFile, subject) = evt.EmailType switch
        {
            EmailType.ConfirmEmail => ("verify.html", "Verify Email"),
            EmailType.ResetPassword => ("resetPassword.html", "Reset Password"),
            _ => throw new InvalidOperationException($"Unsupported email type {evt.EmailType}"),
        };

        var template = await templateProvider.GetTemplateAsync(templateFile, ct);
        var content = string.Format(template, evt.Link);

        await emailSender.SendEmailAsync(FromEmail, FromName, evt.ToEmail, subject, content);

        db.EmailLogs.Add(new EmailLog
        {
            Id = Guid.NewGuid(),
            ToEmail = evt.ToEmail,
            Subject = subject,
            EmailType = evt.EmailType.ToString(),
            SentOn = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task HandleMatchCreatedAsync(Guid messageId, MatchCreatedEvent evt, CancellationToken ct)
    {
        if (!await db.TryRegisterInboxAsync(messageId, ConsumerName, ct))
        {
            return;
        }

        const string subject = "You have a new match";

        await SendMatchEmailAsync(evt.UserA, evt.UserB, subject);
        await SendMatchEmailAsync(evt.UserB, evt.UserA, subject);

        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertContactAsync(Guid messageId, string userId, string email, string userName, CancellationToken ct)
    {
        if (!await db.TryRegisterInboxAsync(messageId, ConsumerName, ct))
        {
            return;
        }

        var contact = await db.UserContacts.FindAsync(new object[] { userId }, ct);

        if (contact is null)
        {
            db.UserContacts.Add(new UserContact
            {
                UserId = userId,
                Email = email,
                UserName = userName,
            });
        }
        else
        {
            contact.Email = email;
            contact.UserName = userName;
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SendMatchEmailAsync(MatchedUser recipient, MatchedUser other, string subject)
    {
        var content = $"<h1>It's a match!</h1><p>You matched with {other.UserName}. Open LOVE.NET to say hi!</p>";

        await emailSender.SendEmailAsync(FromEmail, FromName, recipient.Email, subject, content);

        db.EmailLogs.Add(new EmailLog
        {
            Id = Guid.NewGuid(),
            ToEmail = recipient.Email,
            Subject = subject,
            EmailType = "MatchCreated",
            SentOn = DateTime.UtcNow,
        });
    }
}
