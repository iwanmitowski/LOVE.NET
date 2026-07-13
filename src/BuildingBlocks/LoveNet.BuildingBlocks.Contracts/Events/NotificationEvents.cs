namespace LoveNet.BuildingBlocks.Contracts.Events;

public enum EmailType
{
    ConfirmEmail,
    ResetPassword,
}

/// <summary>
/// Published by Identity when a transactional email must go out; consumed by
/// Notifications, which owns templates and the SendGrid integration. Key: userId.
/// </summary>
public record EmailRequestedEvent(
    EmailType EmailType,
    string ToEmail,
    string ToUserName,
    string Link);
