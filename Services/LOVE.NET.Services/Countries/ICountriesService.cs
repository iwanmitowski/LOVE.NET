namespace LOVE.NET.Services.Countries
{
    using System.Collections.Generic;

    using LOVE.NET.Web.ViewModels.Countries;

    public interface ICountriesService
    {
        public IEnumerable<CountryViewModel> GetAll();

        public CountryCitiesViewModel Get(int id);
    }
}
