namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AutoMapper;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Countries;
    using LOVE.NET.Web.ViewModels.Genders;
    using LOVE.NET.Web.ViewModels.Images;

    public class UserDetailsViewModel : IMapFrom<ApplicationUser>, IHaveCustomMappings
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Bio { get; set; }

        public DateTime Birthdate { get; set; }

        public ICollection<UserMatchViewModel> Matches { get; set; }

        public ICollection<ImageViewModel> Images { get; set; }

        public GenderViewModel Gender { get; set; }

        public CityViewModel City { get; set; }

        public CountryViewModel Country { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            // Getting intersection of both users likes, and select the liked user
            configuration.CreateMap<ApplicationUser, UserDetailsViewModel>()
                .ForMember(m => m.Matches, opt => opt.MapFrom(x =>
                    x.LikesSent
                        .Where(l =>
                            x.LikesReceived.Select(lr => lr.UserId).Intersect(x.LikesSent.Select(ls => ls.LikedUserId)).Contains(l.LikedUserId))
                        .Select(x => x.LikedUser)));
        }
    }
}
