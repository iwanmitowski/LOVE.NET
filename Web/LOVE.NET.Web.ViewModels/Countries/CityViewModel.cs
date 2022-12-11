namespace LOVE.NET.Web.ViewModels.Countries
{
    using AutoMapper;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class CityViewModel : IMapFrom<City>, IHaveCustomMappings
    {
        public int CityId { get; set; }

        public string CityName { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<City, CityViewModel>()
                .ForMember(m => m.CityId, opt => opt.MapFrom(x => x.Id))
                .ForMember(m => m.CityName, opt => opt.MapFrom(x => x.NameAscii));
        }
    }
}
