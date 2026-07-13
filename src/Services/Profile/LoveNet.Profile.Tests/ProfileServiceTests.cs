using System.Text.Json;
using LoveNet.BuildingBlocks.EventBus.Outbox;
using LoveNet.BuildingBlocks.Media;
using LoveNet.Profile.Api.Data;
using LoveNet.Profile.Api.Models;
using LoveNet.Profile.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LoveNet.Profile.Tests;

[TestFixture]
public class ProfileServiceTests
{
    private ProfileDbContext db = null!;
    private ProfileService profileService = null!;
    private UserProfile profile = null!;

    [SetUp]
    public async Task Setup()
    {
        db = TestDb.Create();

        db.Genders.Add(new Gender { Id = 1, Name = "Male" });
        db.Countries.Add(new Country
        {
            Id = 1,
            Name = "Bulgaria",
            Cities = { new City { Id = 1, Name = "София", NameAscii = "Sofia", Latitude = 42.7, Longitude = 23.3, CountryId = 1 } },
        });

        profile = new UserProfile
        {
            UserId = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "user@lovenet.test",
            Bio = "hi",
            Birthdate = DateTime.UtcNow.AddYears(-25),
            GenderId = 1,
            CountryId = 1,
            CityId = 1,
            CreatedOn = DateTime.UtcNow,
            Images =
            {
                new Image { Id = 1, Url = "https://img/1.png", IsProfilePicture = true, CreatedOn = DateTime.UtcNow },
                new Image { Id = 2, Url = "https://img/2.png", CreatedOn = DateTime.UtcNow },
            },
        };
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();

        var imagesService = new Mock<IImagesService>();
        imagesService
            .Setup(s => s.UploadImagesAsync(It.IsAny<IEnumerable<IFormFile>>()))
            .ReturnsAsync(Enumerable.Empty<string>());

        profileService = new ProfileService(db, imagesService.Object, new UserUpdatedPublisher(db));
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public async Task SuccessGetUserDetails()
    {
        var details = await profileService.GetUserDetailsAsync(profile.UserId);

        Assert.That(details, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(details!.UserName, Is.EqualTo("testuser"));
            Assert.That(details.CityName, Is.EqualTo("София"));
            Assert.That(details.Latitude, Is.EqualTo(42.7));
            Assert.That(details.Images, Has.Count.EqualTo(2));
            Assert.That(details.Images[0].IsProfilePicture, Is.True);
        });
    }

    [Test]
    public async Task SuccessEditUserSoftDeletesRemovedImagesAndEmitsUserUpdated()
    {
        // Keep only image 2 and promote it to profile picture; image 1 is soft-deleted.
        var keptImage = JsonSerializer.Serialize(
            new ImageModel { Id = 2, Url = "https://img/2.png", IsProfilePicture = true },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        await profileService.EditUserAsync(
            new EditUserModel
            {
                Id = profile.UserId,
                Email = profile.Email,
                UserName = "renamed",
                Bio = "new bio",
                Birthdate = profile.Birthdate,
                CountryId = 1,
                GenderId = 1,
                CityId = 1,
                Images = new[] { keptImage },
            },
            correlationId: null);

        var updated = await db.UserProfiles.Include(p => p.Images).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updated.UserName, Is.EqualTo("renamed"));
            Assert.That(updated.Images.Single(i => i.Id == 1).IsDeleted, Is.True);
            Assert.That(updated.Images.Single(i => i.Id == 2).IsProfilePicture, Is.True);
        });

        var outbox = await db.Set<OutboxMessage>().SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(outbox.Topic, Is.EqualTo("profile.user-updated"));
            Assert.That(outbox.Payload, Does.Contain("renamed"));
            Assert.That(outbox.Payload, Does.Contain("https://img/2.png"));
            Assert.That(outbox.Payload, Does.Not.Contain("https://img/1.png"));
        });
    }

    [Test]
    public async Task SuccessGetUsersForAdminFiltersAndPages()
    {
        for (var i = 0; i < 12; i++)
        {
            db.UserProfiles.Add(new UserProfile
            {
                UserId = Guid.NewGuid().ToString(),
                UserName = $"extra{i}",
                Email = $"extra{i}@lovenet.test",
                Bio = "filler",
                GenderId = 1,
                CountryId = 1,
                CityId = 1,
                IsBanned = i % 2 == 0,
                CreatedOn = DateTime.UtcNow.AddMinutes(i),
            });
        }

        await db.SaveChangesAsync();

        var firstPage = await profileService.GetUsersForAdminAsync(
            new AdminUsersRequestModel { Page = 1 }, loggedUserId: profile.UserId);

        Assert.Multiple(() =>
        {
            Assert.That(firstPage.TotalUsers, Is.EqualTo(12));
            Assert.That(firstPage.Users.Count(), Is.EqualTo(10));
        });

        var banned = await profileService.GetUsersForAdminAsync(
            new AdminUsersRequestModel { Page = 1, ShowBanned = true }, loggedUserId: profile.UserId);

        Assert.That(banned.TotalUsers, Is.EqualTo(6));

        var searched = await profileService.GetUsersForAdminAsync(
            new AdminUsersRequestModel { Page = 1, Search = "extra1@lovenet" }, loggedUserId: profile.UserId);

        Assert.That(searched.TotalUsers, Is.EqualTo(1));
    }

    [Test]
    public async Task CityBelongsToCountryChecksGeoConsistency()
    {
        Assert.That(await profileService.CityBelongsToCountryAsync(1, 1), Is.True);
        Assert.That(await profileService.CityBelongsToCountryAsync(1, 2), Is.False);
    }
}
