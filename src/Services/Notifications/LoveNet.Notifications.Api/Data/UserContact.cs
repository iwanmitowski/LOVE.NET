namespace LoveNet.Notifications.Api.Data;

/// <summary>Read model of user contact details, kept fresh from identity/profile events.</summary>
public class UserContact
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;
}
