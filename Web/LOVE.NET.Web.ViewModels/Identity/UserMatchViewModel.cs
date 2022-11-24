namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.Collections.Generic;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.Common.Helpers;
    using LOVE.NET.Web.ViewModels.Countries;
    using LOVE.NET.Web.ViewModels.Genders;
    using LOVE.NET.Web.ViewModels.Images;

    public class UserMatchViewModel : IMapFrom<ApplicationUser>
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Bio { get; set; }

        public DateTime Birthdate { get; set; }

        public int Age => DateHelper.AgeCalculator(this.Birthdate);

        public ICollection<ImageViewModel> Images { get; set; }

        public int GenderId { get; set; }

        public string GenderName { get; set; }

        public int CityId { get; set; }

        public string CityName { get; set; }

        public int CountryId { get; set; }

        public string CountryName { get; set; }
    }
}
