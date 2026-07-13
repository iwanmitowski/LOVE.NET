using LoveNet.BuildingBlocks.Web;
using LoveNet.Matching.Api.Models;

namespace LoveNet.Matching.Api.Services;

public interface IMatchingService
{
    Task<List<UserMatchModel>> GetNotSwipedUsersAsync(string userId);

    Task<MatchesResponseModel> GetMatchesAsync(MatchesRequestModel request);

    Task<UserMatchModel?> GetCurrentMatchAsync(string userId);

    /// <summary>Succeeded == a mutual match was created (monolith contract).</summary>
    Task<Result> LikeAsync(string userId, string likedUserId, string? correlationId);
}
