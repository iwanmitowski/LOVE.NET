namespace LOVE.NET.Services.Countries
{
    using System.Collections.Generic;
    using System.Linq;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Countries;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Countries;

    public class CountriesService : ICountriesService
    {
        private readonly ICountriesRepository countriesRepository;

        public CountriesService(ICountriesRepository countriesRepository)
        {
            this.countriesRepository = countriesRepository;
        }

        public IEnumerable<CountryViewModel> GetAll()
        {
            var countriesWithCities = this.countriesRepository.AllAsNoTracking();

            var result = AutoMapperConfig.MapperInstance.Map<IEnumerable<CountryViewModel>>(countriesWithCities).ToList();

            result.Insert(0, new CountryViewModel()
            {
                CountryId = 0,
                CountryName = "Chooce country here",
            });

            return result;
        }

        public CountryCitiesViewModel Get(int id)
        {
            var country = this.countriesRepository
                .WithAllInformation(x => x.Id == id)
                .FirstOrDefault();

            var result = AutoMapperConfig.MapperInstance.Map<CountryCitiesViewModel>(country);

            var orderedCities = result.Cities.OrderBy(c => c.CityName).ToList();
            orderedCities.Insert(0, new CityViewModel()
            {
                CityId = 0,
                CityName = "Choose city here",
            });

            result.Cities = orderedCities;

            return result;
        }
    }
}
