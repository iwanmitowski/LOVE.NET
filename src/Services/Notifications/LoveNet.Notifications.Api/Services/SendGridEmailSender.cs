using SendGrid;
using SendGrid.Helpers.Mail;

namespace LoveNet.Notifications.Api.Services;

/// <summary>
/// SendGrid transactional email sender ported from the monolith. When no API
/// key is configured (local dev) it logs a warning and skips the send instead
/// of throwing.
/// </summary>
public class SendGridEmailSender : IEmailSender
{
    private readonly string? apiKey;
    private readonly ILogger<SendGridEmailSender> logger;

    public SendGridEmailSender(IConfiguration configuration, ILogger<SendGridEmailSender> logger)
    {
        apiKey = configuration["SendGrid:ApiKey"];
        this.logger = logger;
    }

    public async Task SendEmailAsync(
        string from,
        string fromName,
        string to,
        string subject,
        string htmlContent,
        IEnumerable<EmailAttachment>? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(subject) && string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new ArgumentException("Subject and message should be provided.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("SendGrid not configured; skipping email to {To}", to);
            return;
        }

        var client = new SendGridClient(apiKey);
        var fromAddress = new EmailAddress(from, fromName);
        var toAddress = new EmailAddress(to);
        var message = MailHelper.CreateSingleEmail(fromAddress, toAddress, subject, null, htmlContent);

        if (attachments?.Any() == true)
        {
            foreach (var attachment in attachments)
            {
                message.AddAttachment(attachment.FileName, Convert.ToBase64String(attachment.Content), attachment.MimeType);
            }
        }

        var response = await client.SendEmailAsync(message);
        logger.LogInformation("SendGrid responded {StatusCode} for email to {To}", response.StatusCode, to);
    }
}
