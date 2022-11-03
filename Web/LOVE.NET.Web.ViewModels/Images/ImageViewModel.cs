namespace LOVE.NET.Web.ViewModels.Images
{
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class ImageViewModel : IMapFrom<Image>
    {
        public string Url { get; set; }

        public bool IsProfilePicture { get; set; }
    }
}
