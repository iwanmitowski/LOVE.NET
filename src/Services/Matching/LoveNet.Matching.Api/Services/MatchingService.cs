using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Web;
using LoveNet.Matching.Api.Data;
using LoveNet.Matching.Api.Models;
using Microsoft.EntityFrameworkCore;
using Match = LoveNet.Matching.Api.Data.Match;

namespace LoveNet.Matching.Api.Services;

public class MatchingService : IMatchingService
{
    public const string UserNotFound = "User not found";
    public const string YouCantLikeYourself = "You can't like yourself";
    public const string UserAlreadyLiked = "User is already liked";

    private readonly MatchingDbContext db;

    public MatchingService(MatchingDbContext db)
    {
        this.db = db;
    }

    public async Task<List<UserMatchModel>> GetNotSwipedUsersAsync(string userId)
    {
        var candidates = await db.CandidateProfiles.AsNoTracking()
            .Where(c =>
                c.UserId != userId &&
                !c.IsAdmin &&
                !db.Likes.Any(l => l.UserId == userId && l.LikedUserId == c.UserId))
            .ToListAsync();

        return candidates.Select(c => ToModel(c)).ToList();
    }

    public async Task<MatchesResponseModel> GetMatchesAsync(MatchesRequestModel request)
    {
        var userId = request.UserId;

        var matchRooms = await db.Matches.AsNoTracking()
            .Where(m => m.UserAId == userId || m.UserBId == userId)
            .OrderByDescending(m => m.CreatedOn)
            .Select(m => new { OtherUserId = m.UserAId == userId ? m.UserBId : m.UserAId, m.RoomId })
            .ToListAsync();

        var matchedIds = matchRooms.Select(m => m.OtherUserId).ToList();

        var matches = db.CandidateProfiles.AsNoTracking()
            .Where(c => matchedIds.Contains(c.UserId));

        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchTerm = $"%{request.Search.Trim().ToLower()}%";

            matches = matches.Where(c =>
                EF.Functions.Like(c.UserName.ToLower(), searchTerm) ||
                EF.Functions.Like(c.Bio!.ToLower(), searchTerm) ||
                EF.Functions.Like(c.CityName!.ToLower(), searchTerm) ||
                EF.Functions.Like(c.CountryName!.ToLower(), searchTerm) ||
                EF.Functions.Like(c.GenderName!.ToLower(), searchTerm));
        }

        var totalMatches = await matches.CountAsync();

        var candidates = await matches.ToListAsync();

        var page = candidates
            .OrderBy(c => matchedIds.IndexOf(c.UserId)) // newest match first
            .Skip((request.Page - 1) * LoveNetConstants.DefaultTake)
            .Take(LoveNetConstants.DefaultTake)
            .Select(c => ToModel(c, matchRooms.First(m => m.OtherUserId == c.UserId).RoomId))
            .ToList();

        return new MatchesResponseModel
        {
            Matches = page,
            TotalMatches = totalMatches,
        };
    }

    public async Task<UserMatchModel?> GetCurrentMatchAsync(string userId)
    {
        var candidate = await db.CandidateProfiles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return candidate is null ? null : ToModel(candidate);
    }

    public async Task<Result> LikeAsync(string userId, string likedUserId, string? correlationId)
    {
        if (userId == likedUserId)
        {
            return YouCantLikeYourself;
        }

        var likedUser = await db.CandidateProfiles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == likedUserId);

        if (likedUser is null)
        {
            return UserNotFound;
        }

        var alreadyLiked = await db.Likes.AnyAsync(l => l.UserId == userId && l.LikedUserId == likedUserId);

        if (alreadyLiked)
        {
            return UserAlreadyLiked;
        }

        db.Likes.Add(new Like
        {
            UserId = userId,
            LikedUserId = likedUserId,
            CreatedOn = DateTime.UtcNow,
        });

        var isMatch = await db.Likes.AnyAsync(l => l.UserId == likedUserId && l.LikedUserId == userId);

        if (isMatch)
        {
            await CreateMatchAsync(userId, likedUserId, correlationId);
        }

        await db.SaveChangesAsync();

        return isMatch;
    }

    private async Task CreateMatchAsync(string userId, string likedUserId, string? correlationId)
    {
        var match = new Match
        {
            UserAId = userId,
            UserBId = likedUserId,
            RoomId = Match.BuildRoomId(userId, likedUserId),
            CreatedOn = DateTime.UtcNow,
        };

        db.Matches.Add(match);

        var users = await db.CandidateProfiles.AsNoTracking()
            .Where(c => c.UserId == userId || c.UserId == likedUserId)
            .ToDictionaryAsync(c => c.UserId);

        db.AddOutboxMessage(
            KafkaTopics.MatchCreated,
            match.RoomId,
            new MatchCreatedEvent(
                match.Id,
                match.RoomId,
                ToMatchedUser(users[userId]),
                ToMatchedUser(users[likedUserId]),
                match.CreatedOn),
            correlationId);
    }

    private static MatchedUser ToMatchedUser(CandidateProfile c) =>
        new(c.UserId, c.UserName, c.Email, c.AvatarUrl);

    private static UserMatchModel ToModel(CandidateProfile c, string? roomId = null) =>
        new()
        {
            Id = c.UserId,
            UserName = c.UserName,
            Bio = c.Bio,
            Birthdate = c.Birthdate,
            GenderId = c.GenderId,
            GenderName = c.GenderName,
            CityId = c.CityId,
            CityName = c.CityName,
            CountryId = c.CountryId,
            CountryName = c.CountryName,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            RoomId = roomId,
            Images = c.ImageUrls
                .Select((url, index) => new ImageModel
                {
                    Id = index + 1,
                    Url = url,
                    IsProfilePicture = index == 0,
                })
                .ToList(),
        };
}
