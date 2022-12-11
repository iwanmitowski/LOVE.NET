namespace LOVE.NET.Services.Tests
{
    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Identity;
    using LOVE.NET.Services.Images;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;

    using Moq;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;
    using static LOVE.NET.Common.GlobalConstants.EmailMessagesConstants;

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

        [OneTimeTearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
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

        [Test]
        public async Task SuccessAddDefaultProfilePictureUrl()
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

            await identityService.RegisterAsync(request);

            var users = usersRepository.WithAllInformation();
            var test = dbContext.Users;
            var test2 = users;
            var currentUser = users.FirstOrDefault(u => u.Email == "email@email.email");

            Assert.That(currentUser.Images.First().Url, Is.EqualTo(DefaultProfilePictureUrl));
        }

        [Test]
        public async Task SuccessAddDifferentProfilePictureUrl()
        {
            var request = new RegisterViewModel()
            {
                Email = "email@email.email",
                Password = "666",
                ConfirmPassword = "666",
                UserName = "userName",
                Bio = "bio",
                Birthdate = DateTime.UtcNow.AddYears(-19),
                Image = new Mock<IFormFile>().Object,
                CityId = 5878,
                CountryId = 30,
                GenderId = 1,
            };

            await identityService.RegisterAsync(request);

            var users = usersRepository.WithAllInformation();
            var test = dbContext.Users;
            var test2 = users;
            var currentUser = users.FirstOrDefault(u => u.Email == "email@email.email");

            Assert.That(currentUser.Images.First().Url, Is.EqualTo(MockUrl1));
        }

        [Test]
        public async Task SuccessLogin()
        {
            var request = new LoginViewModel()
            {
                Email = "Pet3r@abv.bg",
                Password = "password",
            };

            var result = await identityService.LoginAsync(request);

            Assert.NotNull(result);
        }


        [Test]
        public async Task FailLoginIncorrectPassword()
        {
            userManagerMock.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var request = new LoginViewModel()
            {
                Email = "Pet4r@abv.bg",
                Password = "Wrong password",
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await identityService.LoginAsync(request));
            Assert.That(exception.Message, Is.EqualTo(WrongPassword));
        }

        [Test]
        public async Task FailLoginNotConfirmedEmail()
        {
            var request = new LoginViewModel()
            {
                Email = "Pet4r@abv.bg",
                Password = "Wrong password",
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await identityService.LoginAsync(request));
            Assert.That(exception.Message, Is.EqualTo(EmailNotConfirmed));
        }

        [Test]
        public async Task FailLoginNotExistingEmail()
        {
            var request = new LoginViewModel()
            {
                Email = "aaaaaaaaaaaaar@abv.bg",
                Password = "Wrong password",
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await identityService.LoginAsync(request));
            Assert.That(exception.Message, Is.EqualTo(UserNotFound));
        }

        [Test]
        public async Task FailLoginBannedUser()
        {
            var request = new LoginViewModel()
            {
                Email = "banned@banned.banned",
                Password = "password",
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await identityService.LoginAsync(request));
            Assert.That(exception.Message.StartsWith("You are banned"));
        }

        [Test]
        public void SuccessGenerateRefreshToken()
        {
            var result = identityService.GenerateRefreshToken();

            Assert.IsNotNull(result);
        }

        [Test]
        [TestCase("6666")]
        [TestCase("66666")]
        public void SuccessGetUserDetails(string id)
        {
            var currentUser = users.FirstOrDefault(u => u.Id == id);
            var mappedUser = AutoMapperConfig.MapperInstance.Map<UserDetailsViewModel>(currentUser);
            var result = identityService.GetUserDetails(id);

            Assert.That(mappedUser.Email, Is.EqualTo(mappedUser.Email));
            Assert.That(mappedUser.UserName, Is.EqualTo(mappedUser.UserName));
            Assert.That(mappedUser.Bio, Is.EqualTo(mappedUser.Bio));
            Assert.That(mappedUser.Birthdate, Is.EqualTo(mappedUser.Birthdate));
            Assert.That(mappedUser.Matches.Count, Is.EqualTo(mappedUser.Matches.Count));
            Assert.That(mappedUser.Images.Count, Is.EqualTo(mappedUser.Images.Count));
            Assert.That(mappedUser.GenderId, Is.EqualTo(mappedUser.GenderId));
            Assert.That(mappedUser.CityId, Is.EqualTo(mappedUser.CityId));
            Assert.That(mappedUser.Latitude, Is.EqualTo(mappedUser.Latitude));
            Assert.That(mappedUser.Longitude, Is.EqualTo(mappedUser.Longitude));
        }

        [Test]
        public async Task SuccessEditUser()
        {
            var user = users.FirstOrDefault(u => u.Id == "6666");
            var newImages = new IFormFile[] { new Mock<IFormFile>().Object, new Mock<IFormFile>().Object };

            var request = new EditUserViewModel()
            {
                Id = "6666",
                Email = "email@email.email",
                Password = "666",
                ConfirmPassword = "666",
                UserName = "userName1",
                Bio = "bio2",
                Birthdate = DateTime.UtcNow.AddYears(-19),
                NewImages = newImages,
                Images = new List<string>(),
                CityId = 5878,
                CountryId = 30,
                GenderId = 2,
            };

            await identityService.EditUserAsync(request);
            var result = identityService.GetUserDetails(request.Id);

            Assert.That(result.UserName, Is.EqualTo(request.UserName));
            Assert.That(result.Bio, Is.EqualTo(request.Bio));
            Assert.That(result.Images.Count, Is.Not.EqualTo(request.Images.Count()));
        }
    }
}
