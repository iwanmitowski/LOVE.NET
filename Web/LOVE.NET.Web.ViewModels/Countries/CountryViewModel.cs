namespace LOVE.NET.Web.ViewModels.Countries
{
    using AutoMapper;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class CountryViewModel : IMapFrom<Country>, IHaveCustomMappings
    {
        public int CountryId { get; set; }

        public string CountryName { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Country, CountryViewModel>()
                .ForMember(m => m.CountryId, opt => opt.MapFrom(x => x.Id))
                .ForMember(m => m.CountryName, opt => opt.MapFrom(x => x.Name));
        }
    }
}
