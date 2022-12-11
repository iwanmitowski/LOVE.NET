namespace LOVE.NET.Services.Tests
{
    using LOVE.NET.Data.Common.Repositories;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Genders;

    public class GendersServiceTests : TestsSetUp
    {
        private IGendersService gendersService;
        private IRepository<Gender> gendersRepository;

        [SetUp]
        public async Task Setup()
        {
            await GlobalInitialization();

            gendersRepository = GetIRepository<Gender>();

            gendersService = new GendersService(gendersRepository);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
        }

        [Test]
        public void SuccessGetAll()
        {
            var genders = dbContext.Genders;

            var result = gendersService.GetAll();

            Assert.That(genders.Count(), Is.EqualTo(result.Count()));

            foreach (var gender in genders)
            {
                Assert.That(result.Select(x => x?.Id).Contains(gender.Id));
            }
        }
    }
}
