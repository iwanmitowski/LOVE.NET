namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.Collections.Generic;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Countries;
    using LOVE.NET.Web.ViewModels.Genders;
    using LOVE.NET.Web.ViewModels.Images;

    public class UserMatchViewModel : IMapFrom<ApplicationUser>
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Bio { get; set; }

        public DateTime Birthdate { get; set; }

        public ICollection<ImageViewModel> Images { get; set; }

        public GenderViewModel Gender { get; set; }

        public CityViewModel City { get; set; }
    }
}
