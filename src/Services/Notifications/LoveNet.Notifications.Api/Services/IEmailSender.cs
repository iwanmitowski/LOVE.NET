namespace LoveNet.Notifications.Api.Services;

public interface IEmailSender
{
    Task SendEmailAsync(
        string from,
        string fromName,
        string to,
        string subject,
        string htmlContent,
        IEnumerable<EmailAttachment>? attachments = null);
}

public class EmailAttachment
{
    public byte[] Content { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string MimeType { get; set; } = null!;
}
