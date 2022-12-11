namespace LOVE.NET.Services.Tests
{
    using LOVE.NET.Data.Repositories.Countries;
    using LOVE.NET.Services.Countries;

    public class CountryServiceTests : TestsSetUp
    {
        private ICountriesService countriesService;
        private ICountriesRepository countriesRepository;

        [SetUp]
        public async Task Setup()
        {
            await GlobalInitialization();

            countriesRepository = GetICountriesRepository();

            countriesService = new CountriesService(countriesRepository);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
        }

        [Test]
        public void SuccessGetAll()
        {
            var countriesCount = countries.Count;
            var result = countriesService.GetAll();

            Assert.That(countriesCount + 1, Is.EqualTo(result.Count()));
        }

        [Test]
        [TestCase()]
        public void SuccessGetById()
        {
            var country = countries[0];
            var result = countriesService.Get(country.Id);

            Assert.Multiple(() =>
            {
                Assert.That(country.Id, Is.EqualTo(result.CountryId));
                Assert.That(country.Name, Is.EqualTo(result.CountryName));
                Assert.That(country.Cities.Count + 1, Is.EqualTo(result.Cities.Count()));
            });
        }
    }
}
