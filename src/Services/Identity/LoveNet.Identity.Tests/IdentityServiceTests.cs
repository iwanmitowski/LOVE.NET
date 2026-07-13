using LoveNet.BuildingBlocks.EventBus.Outbox;
using LoveNet.BuildingBlocks.Media;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Identity.Api;
using LoveNet.Identity.Api.Data;
using LoveNet.Identity.Api.Models;
using LoveNet.Identity.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace LoveNet.Identity.Tests;

[TestFixture]
public class IdentityServiceTests
{
    private const string Password = "123456";

    private AppIdentityDbContext db = null!;
    private UserManager<ApplicationUser> userManager = null!;
    private IdentityService identityService = null!;
    private ApplicationUser user = null!;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new AppIdentityDbContext(options);

        var store = new UserStore<ApplicationUser, ApplicationRole, AppIdentityDbContext>(db);
        userManager = new UserManager<ApplicationUser>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);

        var imagesService = new Mock<IImagesService>();
        imagesService
            .Setup(s => s.UploadImagesAsync(It.IsAny<IEnumerable<IFormFile>>()))
            .ReturnsAsync(Enumerable.Empty<string>());

        var jwtOptions = Options.Create(new JwtOptions
        {
            Key = new string('k', 80),
            Issuer = "https://localhost:3000/",
            Audience = "https://localhost:3000/",
        });

        identityService = new IdentityService(userManager, db, imagesService.Object, jwtOptions);

        user = new ApplicationUser
        {
            Email = "user@lovenet.test",
            UserName = "testuser",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(user, Password);
    }

    [TearDown]
    public void TearDown()
    {
        userManager.Dispose();
        db.Dispose();
    }

    [Test]
    public async Task SuccessGenerateJwtToken()
    {
        var token = await identityService.GenerateJwtToken(user);

        Assert.That(token, Is.Not.Null.And.Not.Empty);
        Assert.That(token.Split('.'), Has.Length.EqualTo(3));
    }

    [Test]
    public async Task SuccessLogin()
    {
        var response = await identityService.LoginAsync(new LoginModel
        {
            Email = user.Email!,
            Password = Password,
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(user.Id));
            Assert.That(response.Token, Is.Not.Empty);
            Assert.That(response.IsAdmin, Is.False);
        });
    }

    [Test]
    public async Task SuccessLoginIncludesProfileReadModel()
    {
        db.UserProfiles.Add(new UserProfileLite
        {
            UserId = user.Id,
            Bio = "hello",
            CityName = "Sofia",
            Latitude = 42.7,
            Longitude = 23.3,
            AvatarUrl = "https://img/pfp.png",
        });
        await db.SaveChangesAsync();

        var response = await identityService.LoginAsync(new LoginModel
        {
            Email = user.Email!,
            Password = Password,
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.CityName, Is.EqualTo("Sofia"));
            Assert.That(response.ProfilePicture, Is.EqualTo("https://img/pfp.png"));
            Assert.That(response.Latitude, Is.EqualTo(42.7));
        });
    }

    [Test]
    public void FailLoginIncorrectPassword()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(() => identityService.LoginAsync(new LoginModel
        {
            Email = user.Email!,
            Password = "wrong-password",
        }));

        Assert.That(ex!.Message, Is.EqualTo(IdentityMessages.WrongPassword));
    }

    [Test]
    public async Task FailLoginNotConfirmedEmail()
    {
        user.EmailConfirmed = false;
        await userManager.UpdateAsync(user);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => identityService.LoginAsync(new LoginModel
        {
            Email = user.Email!,
            Password = Password,
        }));

        Assert.That(ex!.Message, Is.EqualTo(IdentityMessages.EmailNotConfirmed));
    }

    [Test]
    public void FailLoginNotExistingEmail()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(() => identityService.LoginAsync(new LoginModel
        {
            Email = "missing@lovenet.test",
            Password = Password,
        }));

        Assert.That(ex!.Message, Is.EqualTo(IdentityMessages.UserNotFound));
    }

    [Test]
    public async Task FailLoginBannedUser()
    {
        user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(1);
        await userManager.UpdateAsync(user);

        Assert.ThrowsAsync<ArgumentException>(() => identityService.LoginAsync(new LoginModel
        {
            Email = user.Email!,
            Password = Password,
        }));
    }

    [Test]
    public void SuccessGenerateRefreshToken()
    {
        var token = identityService.GenerateRefreshToken();

        Assert.Multiple(() =>
        {
            Assert.That(token.Token, Is.Not.Empty);
            Assert.That(token.IsActive, Is.True);
            Assert.That(token.Expires, Is.GreaterThan(DateTime.UtcNow));
        });
    }

    [Test]
    public async Task SuccessRegisterEmitsUserRegisteredOutboxEvent()
    {
        var result = await identityService.RegisterAsync(
            new RegisterModel
            {
                Email = "new@lovenet.test",
                Password = Password,
                ConfirmPassword = Password,
                UserName = "newcomer",
                Bio = "hi",
                Birthdate = DateTime.UtcNow.AddYears(-20),
                GenderId = 1,
                CityId = 1,
                CountryId = 1,
            },
            correlationId: null);

        Assert.That(result.Succeeded, Is.True);

        var outbox = await db.Set<OutboxMessage>().SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(outbox.Topic, Is.EqualTo("identity.user-registered"));
            Assert.That(outbox.Payload, Does.Contain("newcomer"));
            // No uploaded image â†’ the default profile picture leads the list.
            Assert.That(outbox.Payload, Does.Contain(LoveNetConstants.DefaultProfilePictureUrl));
        });
    }

    [Test]
    public async Task SuccessBanUserEmitsUserBannedOutboxEvent()
    {
        var result = await identityService.ModerateAsync(
            new ModerateUserModel { UserId = user.Id, BannedUntil = DateTimeOffset.UtcNow.AddDays(3) },
            correlationId: null);

        Assert.That(result.Succeeded, Is.True);

        var updated = await userManager.FindByIdAsync(user.Id);
        Assert.That(updated!.LockoutEnd, Is.Not.Null);

        var outbox = await db.Set<OutboxMessage>().SingleAsync();
        Assert.That(outbox.Topic, Is.EqualTo("identity.user-banned"));
    }

    [Test]
    public async Task SuccessUnbanUser()
    {
        await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(3));

        var result = await identityService.ModerateAsync(
            new ModerateUserModel { UserId = user.Id, BannedUntil = null },
            correlationId: null);

        Assert.That(result.Succeeded, Is.True);

        var updated = await userManager.FindByIdAsync(user.Id);
        Assert.That(updated!.LockoutEnd, Is.Null);
    }

    [Test]
    public async Task FailModerateUnknownUser()
    {
        var result = await identityService.ModerateAsync(
            new ModerateUserModel { UserId = Guid.NewGuid().ToString(), BannedUntil = null },
            correlationId: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failure, Is.True);
            Assert.That(result.Errors, Does.Contain(IdentityMessages.UserNotFound));
        });
    }
}
