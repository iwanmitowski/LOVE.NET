namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AutoMapper;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Images;

    public class UserDetailsViewModel : IMapFrom<ApplicationUser>, IHaveCustomMappings
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string Bio { get; set; }

        public DateTime Birthdate { get; set; }

        public ICollection<UserMatchViewModel> Matches { get; set; }

        public ICollection<ImageViewModel> Images { get; set; }

        public int GenderId { get; set; }

        public string GenderName { get; set; }

        public int CityId { get; set; }

        public string CityName { get; set; }

        public int CountryId { get; set; }

        public string CountryName { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsBanned { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            // Getting intersection of both users likes, and select the liked user
            configuration.CreateMap<ApplicationUser, UserDetailsViewModel>()
                .ForMember(m => m.Latitude, opt => opt.MapFrom(x => x.City.Latitude))
                .ForMember(m => m.Longitude, opt => opt.MapFrom(x => x.City.Longitude))
                .ForMember(m => m.IsBanned, opt => opt.MapFrom(x => x.LockoutEnd != null))
                .ForMember(m => m.Matches, opt => opt.MapFrom(x =>
                    x.LikesSent
                        .Where(l =>
                            x.LikesReceived.Select(lr => lr.UserId).Intersect(x.LikesSent.Select(ls => ls.LikedUserId)).Contains(l.LikedUserId))
                        .Select(x => x.LikedUser)));
        }
    }
}
