namespace LOVE.NET.Services.Tests
{
    using LOVE.NET.Data.Common.Repositories;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Dashboard;
    using LOVE.NET.Web.ViewModels.Dashboard;

    using Microsoft.AspNetCore.Identity;

    using Moq;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;

    public class DashboardServiceTests : TestsSetUp
    {
        private IDashboardService dashboardService;
        private IUsersRepository usersRepository;
        private IRepository<Image> imagesRepository;
        private IRepository<Message> messagesRepository;
        private Mock<UserManager<ApplicationUser>> userManagerMock;

        [SetUp]
        public async Task Setup()
        {
            await GlobalInitialization();

            usersRepository = GetIUsersRepository();
            imagesRepository = GetIRepository<Image>();
            messagesRepository = GetIRepository<Message>();
            userManagerMock = GetUserManagerMock();


            dashboardService = new DashboardService(
                usersRepository,
                imagesRepository,
                messagesRepository,
                userManagerMock?.Object);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
        }

        [Test]
        public void SuccessGetStatistics()
        {
            var usersCount = dbContext.Users.Count();
            var bannedUsersCount = 1;
            var matchesCount = 2;
            var likedUsersCount = 2;
            var notSwipedUsersCount = usersCount - 2;
            var imagesCount = dbContext.Images.Count();
            var messagesCount = dbContext.Messages.Count();

            var result = dashboardService.GetStatistics();

            Assert.Multiple(() =>
            {
                Assert.That(result.UsersCount, Is.EqualTo(usersCount));
                Assert.That(result.BannedUsersCount, Is.EqualTo(bannedUsersCount));
                Assert.That(result.MatchesCount, Is.EqualTo(matchesCount));
                Assert.That(result.LikedUsersCount, Is.EqualTo(likedUsersCount));
                Assert.That(result.NotSwipedUsersCount, Is.EqualTo(notSwipedUsersCount));
                Assert.That(result.ImagesCount, Is.EqualTo(imagesCount));
                Assert.That(result?.MessagesCount, Is.EqualTo(messagesCount));
            });
        }

        [Test]
        public async Task SuccessGetUsersNoSearchFirstPage()
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Page = 1,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666")?.Id;

            var resultUsers = users
                .Where(u =>
                    u.Email != AdministratorEmail &&
                    u.Id != loggedUserId);
            var totalUsersCount = resultUsers.Count();

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.Multiple(() =>
            {
                Assert.That(result.TotalUsers, Is.EqualTo(totalUsersCount));
                Assert.That(result.Users.Any(u => u.Id == loggedUserId), Is.EqualTo(false));
                Assert.That(result.Users.Any(u => u.Email == AdministratorEmail), Is.EqualTo(false));
                Assert.That(result.Users.Count(), Is.LessThanOrEqualTo(DefaultTake));
            });
        }

        [Test]
        public async Task SuccessGetUsersNoSearchSecondPage()
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Page = 2,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666").Id;

            var resultUsers = users
                .Where(u =>
                    u.Email != AdministratorEmail &&
                    u.Id != loggedUserId);
            var totalUsersCount = resultUsers.Count();

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.Multiple(() =>
            {
                Assert.That(result.TotalUsers, Is.EqualTo(totalUsersCount));
                Assert.That(result.Users.Count(u => u.Id == loggedUserId), Is.EqualTo(0));
                Assert.That(result.Users.Count(u => u.Email == AdministratorEmail), Is.EqualTo(0));
                Assert.That(result.Users.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public async Task SuccessGetBannedUsers()
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = true,
                Page = 2,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666")?.Id;

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.That(result.Users.All(u => u.IsBanned));
        }

        [Test]
        public async Task SuccessGetUsersSearchSecondPage()
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Search = "search",
                Page = 2,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666").Id;

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.That(result.Users.Count(), Is.EqualTo(0));
        }

        [Test]
        [TestCase("Misa4@abv.bg")]
        [TestCase("MISA4@ABV.BG")]
        [TestCase("misa4@abv.bg")]
        [TestCase("        misa4@abv.bg     ")]
        public async Task SuccessGetUsersSearchEmail(string email)
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Search = email,
                Page = 1,
            };

            var loggedUserId = users?.FirstOrDefault(u => u.Id == "6666").Id;

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.That(result.Users.All(u => u.Email.ToLower().Contains(email.Trim().ToLower())), Is.True);
        }

        [Test]
        [TestCase("Misa ")]
        [TestCase("MISA")]
        [TestCase("  misa")]
        public async Task SuccessGetUsersSearchUserName(string userName)
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Search = userName,
                Page = 1,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666").Id;

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.That(result.Users.All(u => u.UserName.ToLower().Contains(userName.Trim().ToLower())));
        }

        [Test]
        [TestCase("UwU ")]
        [TestCase("Uwu")]
        [TestCase("  UWU")]
        public async Task SuccessGetUsersSearchBio(string bio)
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Search = bio,
                Page = 1,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666").Id;

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.That(result.Users.All(u => u.Bio.ToLower().Contains(bio.Trim().ToLower())));
        }

        [Test]
        [TestCase("Svil")]
        [TestCase("Svilengrad  ")]
        [TestCase("Sv")]
        public async Task SuccessGetUsersSearchCity(string cityName)
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Search = cityName,
                Page = 1,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666").Id;

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.That(result.Users.All(u => u.CityName.ToLower().Contains(cityName.Trim().ToLower())));
        }

        [Test]
        [TestCase("Bul")]
        [TestCase("bul   ")]
        [TestCase("b")]
        [TestCase("BUL")]
        public async Task SuccessGetUsersSearchCountry(string countryName)
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Search = countryName,
                Page = 1,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666").Id;

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.That(result.Users.All(u => u.CountryName.ToLower().Contains(countryName.Trim().ToLower())));
        }

        [Test]
        [TestCase("Male")]
        [TestCase("Male ")]
        [TestCase("male ")]
        [TestCase("  MALE ")]
        [TestCase("Female")]
        [TestCase("FEMALE ")]
        [TestCase("female ")]
        [TestCase("female ")]
        [TestCase("Trans")]
        [TestCase("Trans  ")]
        [TestCase("trans  ")]
        [TestCase("TRANS  ")]
        public async Task SuccessGetUsersSearchGender(string genderName)
        {
            var users = dbContext.Users;

            var request = new DashboardUserRequestViewModel()
            {
                ShowBanned = false,
                Search = genderName,
                Page = 1,
            };

            var loggedUserId = users.FirstOrDefault(u => u.Id == "6666")?.Id;

            var result = await dashboardService.GetUsersAsync(request, loggedUserId);

            Assert.That(result.Users.All(u => u.GenderName.ToLower().Contains(genderName.Trim().ToLower())));
        }

        [Test]
        public async Task SuccessBanUser()
        {
            var user = users.FirstOrDefault(u => u.Id == "6666");

            var request = new ModerateUserViewModel()
            {
                BannedUntil = DateTime.UtcNow.AddYears(666),
                UserId = "6666",
            };

            var result = await dashboardService.ModerateAsync(request);
            var userFromDb = dbContext.Users.FirstOrDefault(u => u.Id == user.Id);

            Assert.Multiple(() =>
            {
                Assert.That(result.Succeeded);
                Assert.That(userFromDb?.LockoutEnd, Is.Not.EqualTo(null));
            });
        }

        [Test]
        public async Task SuccessUnbanUser()
        {
            var user = users.FirstOrDefault(u => u.Id == "666666");

            var request = new ModerateUserViewModel()
            {
                BannedUntil = null,
                UserId = "666666",
            };

            var result = await dashboardService.ModerateAsync(request);
            var userFromDb = dbContext.Users.FirstOrDefault(u => u.Id == user.Id);

            Assert.Multiple(() =>
            {
                Assert.That(result.Succeeded);
                Assert.That(userFromDb?.LockoutEnd, Is.EqualTo(null));
            });
        }

        [Test]
        public async Task FailBanUnbanUser()
        {
            var request = new ModerateUserViewModel()
            {
                BannedUntil = null,
                UserId = "666666666666",
            };

            var result = await dashboardService.ModerateAsync(request);

            Assert.That(result?.Errors, Does.Contain(UserNotFound));
        }

        [Test]
        public async Task FailSetBanEndingDate()
        {
            userManagerMock.Setup(x =>
              x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()))
                  .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

            var request = new ModerateUserViewModel()
            {
                BannedUntil = DateTime.UtcNow,
                UserId = "6666",
            };

            var result = await dashboardService.ModerateAsync(request);

            Assert.That(result?.Errors, Does.Contain(UserCouldNotBeBanned));
        }
    }
}
