namespace LOVE.NET.Services.Countries
{
    using System.Collections.Generic;

    using LOVE.NET.Web.ViewModels.Countries;

    public interface ICountriesService
    {
        IEnumerable<CountryViewModel> GetAll();

        CountryCitiesViewModel Get(int id);
    }
}
