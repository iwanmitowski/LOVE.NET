namespace LOVE.NET.Services.Countries
{
    using System.Collections.Generic;
    using System.Linq;
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

            var result = AutoMapperConfig.MapperInstance.Map<IEnumerable<CountryViewModel>>(countriesWithCities);

            return result;
        }

        public CountryCitiesViewModel Get(int id)
        {
            var country = this.countriesRepository
                .WithAllInformation(x => x.Id == id)
                .FirstOrDefault();

            var result = AutoMapperConfig.MapperInstance.Map<CountryCitiesViewModel>(country);

            return result;
        }
    }
}
