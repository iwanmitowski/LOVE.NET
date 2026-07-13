using LoveNet.Profile.Api.Data;
using LoveNet.Profile.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Api.Services;

public interface ICountriesService
{
    Task<List<CountryModel>> GetAllAsync();

    Task<CountryCitiesModel?> GetAsync(int id);
}

public class CountriesService : ICountriesService
{
    private readonly ProfileDbContext db;

    public CountriesService(ProfileDbContext db)
    {
        this.db = db;
    }

    public async Task<List<CountryModel>> GetAllAsync()
    {
        var countries = await db.Countries.AsNoTracking()
            .Select(c => new CountryModel { CountryId = c.Id, CountryName = c.Name })
            .ToListAsync();

        // Placeholder option the SPA dropdown expects at index 0.
        countries.Insert(0, new CountryModel { CountryId = 0, CountryName = "Chooce country here" });

        return countries;
    }

    public async Task<CountryCitiesModel?> GetAsync(int id)
    {
        var country = await db.Countries.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CountryCitiesModel
            {
                CountryId = c.Id,
                CountryName = c.Name,
                Cities = c.Cities
                    .Select(city => new CityModel { CityId = city.Id, CityName = city.NameAscii })
                    .ToList(),
            })
            .FirstOrDefaultAsync();

        if (country is null)
        {
            return null;
        }

        var orderedCities = country.Cities.OrderBy(c => c.CityName).ToList();
        orderedCities.Insert(0, new CityModel { CityId = 0, CityName = "Choose city here" });
        country.Cities = orderedCities;

        return country;
    }
}
