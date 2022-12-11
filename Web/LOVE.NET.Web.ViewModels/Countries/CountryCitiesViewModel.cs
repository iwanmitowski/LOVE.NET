namespace LOVE.NET.Web.ViewModels.Countries
{
    using System.Collections.Generic;

    using AutoMapper;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class CountryCitiesViewModel : IMapFrom<Country>, IHaveCustomMappings
    {
        public int CountryId { get; set; }

        public string CountryName { get; set; }

        public IEnumerable<CityViewModel> Cities { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Country, CountryCitiesViewModel>()
                .ForMember(m => m.CountryId, opt => opt.MapFrom(x => x.Id))
                .ForMember(m => m.CountryName, opt => opt.MapFrom(x => x.Name));
        }
    }
}
