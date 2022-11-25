﻿namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.Text.Json.Serialization;

    using AutoMapper;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Countries;

    // TODO: Load Images, Matches, Likes
    public class LoginResponseModel : IMapFrom<ApplicationUser>, IHaveCustomMappings
    {
        public string Id { get; set; }

        public string Token { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public bool IsAdmin { get; set; }

        public string Bio { get; set; }

        public DateTime Birthdate { get; set; }

        public string CountryName { get; set; }

        public string CityName { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<ApplicationUser, LoginResponseModel>()
                .ForMember(m => m.Latitude, opt => opt.MapFrom(x => x.City.Latitude))
                .ForMember(m => m.Longitude, opt => opt.MapFrom(x => x.City.Longitude));
        }
    }
}
