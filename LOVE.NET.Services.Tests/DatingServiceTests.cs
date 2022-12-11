namespace LOVE.NET.Services.Tests
{
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Dating;
    using LOVE.NET.Web.ViewModels.Dating;

    using Microsoft.EntityFrameworkCore;

    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;

    public class DatingServiceTests : TestsSetUp
    {
        private IDatingService datingService;
        private IUsersRepository usersRepository;

        [SetUp]
        public async Task Setup()
        {
            await GlobalInitialization();
            usersRepository = GetIUsersRepository();

            datingService = new DatingService(usersRepository);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
        }

        [Test]
        public void SuccessGetNotSwipedUsers()
        {
            var totalUsers = users.Count;
            var notSwipedUsers = datingService.GetNotSwipedUsers("6666");

            // Excluding itself and the admin and it's like 
            var expectedNotSwiped = users.Count - 3;
            Assert.Multiple(() =>
            {
                Assert.That(notSwipedUsers.Count(), Is.EqualTo(expectedNotSwiped));
                Assert.That(notSwipedUsers.Count(u => u.UserName == "admin"), Is.EqualTo(0));
                Assert.That(notSwipedUsers.Count(u => u.Id == "6666"), Is.EqualTo(0));
            });
        }

        [Test]
        public void SuccessGetMatchesFirstPage()
        {
            var request = new MatchesRequestViewModel()
            {
                UserId = "6666",
                Page = 1,
            };

            var result = datingService.GetMatches(request);

            Assert.Multiple(() =>
            {
                Assert.That(result.TotalMatches, Is.EqualTo(1));
                Assert.That(result.Matches.Count(), Is.EqualTo(1));
            });
        }

        [Test]
        public void SuccessGetMatchesSecondPage()
        {
            var request = new MatchesRequestViewModel()
            {
                UserId = "6666",
                Page = 2,
            };

            var result = datingService.GetMatches(request);

            Assert.Multiple(() =>
            {
                Assert.That(result.TotalMatches, Is.EqualTo(1));
                Assert.That(result.Matches.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void SuccessGetMatcheModelOnLike()
        {
            var result = datingService.GetCurrentMatch("66666");

            Assert.That(result.Id, Is.EqualTo("66666"));
        }

        [Test]
        public async Task SuccessLikeUserNoMatch()
        {
            var users = dbContext.Users.Include(u => u.LikesSent).Where(u => u.Id == "6666" || u.Id == "77777");
            var result = await datingService.LikeAsync("6666", "77777");

            var isUserLiked = users.FirstOrDefault(u => u.Id == "6666")?.LikesSent
                .Any(u => u.LikedUserId == "77777");

            Assert.Multiple(() =>
            {
                Assert.That(result.Failure, Is.True);
                Assert.That(isUserLiked, Is.True);
            });
        }

        [Test]
        public async Task SuccessLikeUserMatch()
        {
            var users = dbContext.Users.Include(u => u.LikesSent).Where(u => u.Id == "6666" || u.Id == "77777");
            var firstLike = await datingService.LikeAsync("6666", "77777");
            var matchLike = await datingService.LikeAsync("77777", "6666");

            var isMatch = users?.FirstOrDefault(u => u.Id == "77777")?.LikesSent.Any(ls => ls.LikedUserId == "6666");
            Assert.Multiple(() =>
            {
                Assert.That(firstLike.Failure, Is.True);
                Assert.That(matchLike.Succeeded, Is.True);
                Assert.That(isMatch, Is.True);
            });
        }

        [Test]
        public async Task FailLikeUserNotExisting()
        {
            var result = await datingService.LikeAsync("6666", "88888");
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors, Is.Not.Empty);
                Assert.That(result.Errors, Does.Contain(UserNotFound));
            });
        }

        [Test]
        public async Task FailLikeUserAgain()
        {
            var result = await datingService.LikeAsync("6666", "66666");

            Assert.That(result.Errors, Is.Not.Empty);
            Assert.That(result.Errors, Does.Contain(UserAlreadyLiked));
        }

        [Test]
        public async Task FailLikeUserLikingYourself()
        {
            var result = await datingService.LikeAsync("6666", "6666");

            Assert.That(result.Errors, Is.Not.Empty);
            Assert.That(result.Errors, Does.Contain(YouCantLikeYourself));
        }
    }
}
