namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.Collections.Generic;

    using AutoMapper;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.Common.Helpers;
    using LOVE.NET.Web.ViewModels.Images;

    public class UserMatchViewModel : IMapFrom<ApplicationUser>, IHaveCustomMappings
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

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<ApplicationUser, UserMatchViewModel>()
                .ForMember(m => m.Latitude, opt => opt.MapFrom(x => x.City.Latitude))
                .ForMember(m => m.Longitude, opt => opt.MapFrom(x => x.City.Longitude));
        }
    }
}
