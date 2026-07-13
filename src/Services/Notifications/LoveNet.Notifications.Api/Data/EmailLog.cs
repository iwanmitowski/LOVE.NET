namespace LoveNet.Notifications.Api.Data;

/// <summary>One row per successfully sent email.</summary>
public class EmailLog
{
    public Guid Id { get; set; }

    public string ToEmail { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string EmailType { get; set; } = null!;

    public DateTime SentOn { get; set; }
}
