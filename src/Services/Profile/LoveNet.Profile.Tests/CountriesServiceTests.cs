using LoveNet.Profile.Api.Data;
using LoveNet.Profile.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Tests;

[TestFixture]
public class CountriesServiceTests
{
    private ProfileDbContext db = null!;
    private CountriesService countriesService = null!;

    [SetUp]
    public async Task Setup()
    {
        db = TestDb.Create();

        db.Countries.Add(new Country
        {
            Id = 1,
            Name = "Bulgaria",
            Cities =
            {
                new City { Id = 2, Name = "София", NameAscii = "Sofia", Latitude = 42.7, Longitude = 23.3, CountryId = 1 },
                new City { Id = 1, Name = "Пловдив", NameAscii = "Plovdiv", Latitude = 42.15, Longitude = 24.75, CountryId = 1 },
            },
        });
        await db.SaveChangesAsync();

        countriesService = new CountriesService(db);
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public async Task SuccessGetAllPrependsPlaceholder()
    {
        var countries = await countriesService.GetAllAsync();

        Assert.Multiple(() =>
        {
            Assert.That(countries, Has.Count.EqualTo(2));
            Assert.That(countries[0].CountryId, Is.EqualTo(0));
            Assert.That(countries[1].CountryName, Is.EqualTo("Bulgaria"));
        });
    }

    [Test]
    public async Task SuccessGetByIdReturnsOrderedCities()
    {
        var country = await countriesService.GetAsync(1);

        Assert.That(country, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(country!.Cities, Has.Count.EqualTo(3));
            Assert.That(country.Cities[0].CityId, Is.EqualTo(0));
            Assert.That(country.Cities[1].CityName, Is.EqualTo("Plovdiv"));
            Assert.That(country.Cities[2].CityName, Is.EqualTo("Sofia"));
        });
    }

    [Test]
    public async Task GetByIdReturnsNullForUnknownCountry()
    {
        Assert.That(await countriesService.GetAsync(999), Is.Null);
    }
}
