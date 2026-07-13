namespace LoveNet.BuildingBlocks.Web;

/// <summary>Constants shared by more than one service. Service-specific
/// messages/routes live inside the owning service.</summary>
public static class LoveNetConstants
{
    public const string AdministratorRoleName = "Administrator";

    public const int DefaultTake = 10;

    public const int MinimalAge = 18;

    public const int MaxFileSizeInBytes = 20 * 1024 * 1024;

    public const string DefaultProfilePictureUrl = "https://res.cloudinary.com/dojl8gfnd/image/upload/v1666714042/24-248253_user-profile-default-image-png-clipart-png-download_tuamuf.png";
}
