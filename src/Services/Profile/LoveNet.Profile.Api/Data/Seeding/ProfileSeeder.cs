using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Api.Data.Seeding;

/// <summary>Seeds the geo/gender reference data from the monolith's CSV files.</summary>
public static class ProfileSeeder
{
    public static async Task SeedAsync(ProfileDbContext db)
    {
        await SeedGendersAsync(db);
        await SeedCountriesAndCitiesAsync(db);
    }

    private static async Task SeedGendersAsync(ProfileDbContext db)
    {
        if (await db.Genders.AnyAsync())
        {
            return;
        }

        db.Genders.AddRange(
            new Gender { Name = "Male" },
            new Gender { Name = "Female" },
            new Gender { Name = "Other" });

        await db.SaveChangesAsync();
    }

    private static async Task SeedCountriesAndCitiesAsync(ProfileDbContext db)
    {
        if (await db.Countries.AnyAsync())
        {
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };

        using (var reader = new StreamReader(SeedFile("Countries.csv")))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<CountriesCsvMap>();
            db.Countries.AddRange(csv.GetRecords<Country>().ToList());
            await db.SaveChangesAsync();
        }

        using (var reader = new StreamReader(SeedFile("CitiesWithCountries.csv")))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<CitiesCsvMap>();
            db.Cities.AddRange(csv.GetRecords<City>().ToList());
            await db.SaveChangesAsync();
        }
    }

    private static string SeedFile(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Data", "Files", fileName);

    private sealed class CountriesCsvMap : ClassMap<Country>
    {
        private CountriesCsvMap()
        {
            Map(m => m.Id).Ignore();
            Map(m => m.Name);
        }
    }

    private sealed class CitiesCsvMap : ClassMap<City>
    {
        private CitiesCsvMap()
        {
            Map(m => m.Name);
            Map(m => m.NameAscii);
            Map(m => m.Latitude);
            Map(m => m.Longitude);
            Map(m => m.CountryId);
        }
    }
}
