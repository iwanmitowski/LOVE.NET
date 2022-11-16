namespace LOVE.NET.Web.ViewModels.Identity
{
    using System.Collections.Generic;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Images;

    public class EditUserViewModel : RegisterViewModel, IMapTo<ApplicationUser>
    {
        public string Id { get; set; }

        public IEnumerable<ImageViewModel> Images { get; set; } = new HashSet<ImageViewModel>();
    }
}
