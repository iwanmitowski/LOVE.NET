namespace LOVE.NET.Data.Seeding
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CsvHelper;
    using CsvHelper.Configuration;

    using LOVE.NET.Data.Models;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.FilePaths;

    internal class CountriesSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var isPopulated = dbContext.Countries.Any();

            if (isPopulated)
            {
                return;
            }

            await this.SeedCountriesAsync(dbContext);
            await this.SeedCitiesAsync(dbContext);
        }

        private async Task SeedCountriesAsync(ApplicationDbContext dbContext)
        {
            var filePath = Path.Combine(
                    OneDirectoryUp,
                    OneDirectoryUp,
                    OneDirectoryUp,
                    SystemName,
                    Data,
                    SystemNameData,
                    Files,
                    CountriesFileName);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<CountriesCsvMap>();
                var countries = csv.GetRecords<Country>().ToList();

                await dbContext.Countries.AddRangeAsync(countries);
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task SeedCitiesAsync(ApplicationDbContext dbContext)
        {
            var filePath = Path.Combine(
                    OneDirectoryUp,
                    OneDirectoryUp,
                    OneDirectoryUp,
                    SystemName,
                    Data,
                    SystemNameData,
                    Files,
                    CitiesWithCountriesFileName);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<CitiesCsvMap>();
                var cities = csv.GetRecords<City>().ToList();

                await dbContext.Cities.AddRangeAsync(cities);
            }
        }

        private sealed class CountriesCsvMap : ClassMap<Country>
        {
            private CountriesCsvMap()
            {
                this.Map(m => m.Id).Ignore();
                this.Map(m => m.Name);
            }
        }

        private sealed class CitiesCsvMap : ClassMap<City>
        {
            private CitiesCsvMap()
            {
                this.Map(m => m.Name);
                this.Map(m => m.NameAscii);
                this.Map(m => m.Latitude);
                this.Map(m => m.Longitude);
                this.Map(m => m.CountryId);
            }
        }
    }
}
