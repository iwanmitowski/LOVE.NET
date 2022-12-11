using LOVE.NET.Data.Repositories.Users;
using LOVE.NET.Services.Dating;
using LOVE.NET.Web.ViewModels.Dating;

using Microsoft.EntityFrameworkCore;

namespace LOVE.NET.Services.Tests
{
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

            Assert.AreEqual(expectedNotSwiped, notSwipedUsers.Count());
            Assert.That(notSwipedUsers.Count(u => u.UserName == "admin"), Is.EqualTo(0));
            Assert.That(notSwipedUsers.Count(u => u.Id == "6666"), Is.EqualTo(0));
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

            Assert.That(result.TotalMatches, Is.EqualTo(1));
            Assert.That(result.Matches.Count(), Is.EqualTo(1));
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

            Assert.That(result.TotalMatches, Is.EqualTo(1));
            Assert.That(result.Matches.Count(), Is.EqualTo(0));
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

            var isUserLiked = users.FirstOrDefault(u => u.Id == "6666")
                .LikesSent.Any(u => u.LikedUserId == "77777");

            Assert.True(result.Failure);
            Assert.True(isUserLiked);
        }

        [Test]
        public async Task SuccessLikeUserMatch()
        {
            var users = dbContext.Users.Include(u => u.LikesSent).Where(u => u.Id == "6666" || u.Id == "77777");
            var firstLike = await datingService.LikeAsync("6666", "77777");
            var matchLike = await datingService.LikeAsync("77777", "6666");

            var isMatch = users.FirstOrDefault(u => u.Id == "77777").LikesSent.Any(ls => ls.LikedUserId == "6666");

            Assert.True(firstLike.Failure);
            Assert.True(matchLike.Succeeded);
            Assert.True(isMatch);
        }

        [Test]
        public async Task FailLikeUserNotExisting()
        {
            var result = await datingService.LikeAsync("6666", "88888");

            Assert.That(result.Errors.Count(), Is.GreaterThan(0));
            Assert.True(result.Errors.Contains(UserNotFound));
        }

        [Test]
        public async Task FailLikeUserAgain()
        {
            var result = await datingService.LikeAsync("6666", "66666");

            Assert.That(result.Errors.Count(), Is.GreaterThan(0));
            Assert.True(result.Errors.Contains(UserAlreadyLiked));
        }

        [Test]
        public async Task FailLikeUserLikingYourself()
        {
            var result = await datingService.LikeAsync("6666", "6666");

            Assert.That(result.Errors.Count(), Is.GreaterThan(0));
            Assert.True(result.Errors.Contains(YouCantLikeYourself));
        }
    }
}
