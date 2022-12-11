namespace LOVE.NET.Services.Dating
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Web.ViewModels.Dating;
    using LOVE.NET.Web.ViewModels.Identity;

    public interface IDatingService
    {
        IEnumerable<UserMatchViewModel> GetNotSwipedUsers(string userId);

        UserMatchViewModel GetCurrentMatch(string userId);

        MatchesViewModel GetMatches(MatchesRequestViewModel request);

        Task<Result> LikeAsync(string userId, string likedUserId);
    }
}
