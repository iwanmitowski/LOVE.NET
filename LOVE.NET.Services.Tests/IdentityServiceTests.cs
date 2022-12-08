using Castle.Core.Configuration;

using LOVE.NET.Data.Models;
using LOVE.NET.Data.Repositories.Users;
using LOVE.NET.Services.Identity;
using LOVE.NET.Services.Images;
using LOVE.NET.Web.ViewModels.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using Moq;

namespace LOVE.NET.Services.Tests
{
    public class IdentityServiceTests : TestsSetUp
    {
        private IIdentityService identityService;
        private Mock<UserManager<ApplicationUser>> userManagerMock;
        private IConfigurationRoot configuration;
        private IUsersRepository usersRepository;
        private IImagesService imagesService;

        [SetUp]
        public async Task Setup()
        {
            await GlobalInitialization();

            userManagerMock = GetUserManagerMock();
            configuration = GetIConfiguration();
            usersRepository = GetIUsersRepository();
            imagesService = GetIImagesService();

            identityService = new IdentityService(
                userManagerMock.Object,
                configuration,
                usersRepository,
                imagesService);
        }

        [Test]
        public async Task SuccessGenerateJwtToken()
        {
            var user = users[2];
            var token = await identityService.GenerateJwtToken(user);

            Assert.IsNotNull(token);
        }

        [Test]
        public async Task FailRegister()
        {
            userManagerMock.Setup(x =>
              x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                  .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

            var request = new RegisterViewModel()
            {
                Email = "valid@email.com",
                Password = "666",
                ConfirmPassword = "666",
                UserName = "valid@email.com",
                Bio = "bio",
                Birthdate = DateTime.UtcNow.AddYears(-19),
                CityId = 5878,
                CountryId = 30,
                GenderId = 1,
            };

            var response = await identityService.RegisterAsync(request);

            Assert.True(response.Errors.Any());
        }


        //[TestCase("admin@admin.admin", "password", "password", "userName", "bio", "12.30.2000", 5878, 30, 1)]
        //[TestCase("email", "password", "password", "admin", "bio", "12.30.2000", 5878, 30, 1)]
        [Test]
        public async Task SuccessRegister()
        {
            var request = new RegisterViewModel()
            {
                Email = "email@email.email",
                Password = "666",
                ConfirmPassword = "666",
                UserName = "userName",
                Bio = "bio",
                Birthdate = DateTime.UtcNow.AddYears(-19),
                CityId = 5878,
                CountryId = 30,
                GenderId = 1,
            };

            var response = await identityService.RegisterAsync(request);

            Assert.True(response.Succeeded);
        }

    }
}
