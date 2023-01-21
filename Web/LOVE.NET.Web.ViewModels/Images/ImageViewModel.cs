namespace LOVE.NET.Web.ViewModels.Images
{
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class ImageViewModel : IMapFrom<Image>, IMapTo<Image>
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public bool IsProfilePicture { get; set; }

        public bool IsDeleted { get; set; }
    }
}
