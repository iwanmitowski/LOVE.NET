namespace LoveNet.Profile.Api.Data;

public class Image
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public bool IsProfilePicture { get; set; }

    /// <summary>Soft delete, preserved from the monolith's edit flow.</summary>
    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public string UserId { get; set; } = null!;
}
