namespace LOVE.NET.Web.ViewModels.Identity
{
    using System.Collections.Generic;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class EditUserViewModel : RegisterViewModel, IMapTo<ApplicationUser>
    {
        public string Id { get; set; }

        public IEnumerable<string> Images { get; set; }
    }
}
