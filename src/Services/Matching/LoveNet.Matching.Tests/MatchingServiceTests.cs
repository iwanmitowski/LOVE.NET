using LoveNet.BuildingBlocks.EventBus.Outbox;
using LoveNet.Matching.Api.Data;
using LoveNet.Matching.Api.Models;
using LoveNet.Matching.Api.Services;
using Microsoft.EntityFrameworkCore;
using Match = LoveNet.Matching.Api.Data.Match;

namespace LoveNet.Matching.Tests;

[TestFixture]
public class MatchingServiceTests
{
    private MatchingDbContext db = null!;
    private MatchingService matchingService = null!;

    private CandidateProfile me = null!;
    private CandidateProfile other = null!;
    private CandidateProfile admin = null!;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<MatchingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new MatchingDbContext(options);

        me = NewCandidate("alpha", "Sofia");
        other = NewCandidate("beta", "Plovdiv");
        admin = NewCandidate("admin", "Sofia");
        admin.IsAdmin = true;

        db.CandidateProfiles.AddRange(me, other, admin);
        await db.SaveChangesAsync();

        matchingService = new MatchingService(db);
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public async Task SuccessGetNotSwipedUsersExcludesSelfAdminsAndSwiped()
    {
        var deck = await matchingService.GetNotSwipedUsersAsync(me.UserId);

        Assert.That(deck.Select(u => u.Id), Is.EqualTo(new[] { other.UserId }));

        await matchingService.LikeAsync(me.UserId, other.UserId, null);

        deck = await matchingService.GetNotSwipedUsersAsync(me.UserId);
        Assert.That(deck, Is.Empty);
    }

    [Test]
    public async Task SuccessLikeUserNoMatch()
    {
        var result = await matchingService.LikeAsync(me.UserId, other.UserId, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False); // no reciprocal like yet
            Assert.That(result.Errors, Is.Empty);
        });
        Assert.That(await db.Matches.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task SuccessLikeUserMatchPersistsMatchAndEmitsEvent()
    {
        await matchingService.LikeAsync(other.UserId, me.UserId, null);
        var result = await matchingService.LikeAsync(me.UserId, other.UserId, null);

        Assert.That(result.Succeeded, Is.True);

        var match = await db.Matches.SingleAsync();
        var expectedRoomId = Match.BuildRoomId(me.UserId, other.UserId);
        Assert.That(match.RoomId, Is.EqualTo(expectedRoomId));

        var outbox = await db.Set<OutboxMessage>().SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(outbox.Topic, Is.EqualTo("matching.match-created"));
            Assert.That(outbox.Key, Is.EqualTo(expectedRoomId));
            Assert.That(outbox.Payload, Does.Contain(me.UserId).And.Contain(other.UserId));
        });
    }

    [Test]
    public async Task FailLikeUserNotExisting()
    {
        var result = await matchingService.LikeAsync(me.UserId, Guid.NewGuid().ToString(), null);

        Assert.That(result.Errors, Does.Contain(MatchingService.UserNotFound));
    }

    [Test]
    public async Task FailLikeUserAgain()
    {
        await matchingService.LikeAsync(me.UserId, other.UserId, null);
        var result = await matchingService.LikeAsync(me.UserId, other.UserId, null);

        Assert.That(result.Errors, Does.Contain(MatchingService.UserAlreadyLiked));
    }

    [Test]
    public async Task FailLikeUserLikingYourself()
    {
        var result = await matchingService.LikeAsync(me.UserId, me.UserId, null);

        Assert.That(result.Errors, Does.Contain(MatchingService.YouCantLikeYourself));
    }

    [Test]
    public async Task SuccessGetMatchesReadsPersistedMatchesWithRoomIds()
    {
        await matchingService.LikeAsync(other.UserId, me.UserId, null);
        await matchingService.LikeAsync(me.UserId, other.UserId, null);

        var matches = await matchingService.GetMatchesAsync(new MatchesRequestModel
        {
            UserId = me.UserId,
            Page = 1,
        });

        Assert.Multiple(() =>
        {
            Assert.That(matches.TotalMatches, Is.EqualTo(1));
            Assert.That(matches.Matches.Single().Id, Is.EqualTo(other.UserId));
            Assert.That(matches.Matches.Single().RoomId, Is.EqualTo(Match.BuildRoomId(me.UserId, other.UserId)));
        });
    }

    [Test]
    public async Task SuccessGetMatchesSearchFiltersByCity()
    {
        await matchingService.LikeAsync(other.UserId, me.UserId, null);
        await matchingService.LikeAsync(me.UserId, other.UserId, null);

        var hit = await matchingService.GetMatchesAsync(new MatchesRequestModel
        {
            UserId = me.UserId,
            Page = 1,
            Search = "plovdiv",
        });
        var miss = await matchingService.GetMatchesAsync(new MatchesRequestModel
        {
            UserId = me.UserId,
            Page = 1,
            Search = "varna",
        });

        Assert.Multiple(() =>
        {
            Assert.That(hit.TotalMatches, Is.EqualTo(1));
            Assert.That(miss.TotalMatches, Is.Zero);
        });
    }

    [Test]
    public async Task ImagesAreSynthesizedProfilePictureFirst()
    {
        var deck = await matchingService.GetNotSwipedUsersAsync(me.UserId);
        var images = deck.Single().Images;

        Assert.Multiple(() =>
        {
            Assert.That(images, Has.Count.EqualTo(2));
            Assert.That(images[0].IsProfilePicture, Is.True);
            Assert.That(images[0].Url, Does.Contain("pfp"));
        });
    }

    private static CandidateProfile NewCandidate(string name, string city) =>
        new()
        {
            UserId = Guid.NewGuid().ToString(),
            UserName = name,
            Email = $"{name}@lovenet.test",
            Bio = $"{name} bio",
            Birthdate = DateTime.UtcNow.AddYears(-25),
            GenderId = 1,
            GenderName = "Male",
            CityId = 1,
            CityName = city,
            CountryId = 1,
            CountryName = "Bulgaria",
            ImageUrls = new List<string> { $"https://img/{name}-pfp.png", $"https://img/{name}-2.png" },
        };
}
