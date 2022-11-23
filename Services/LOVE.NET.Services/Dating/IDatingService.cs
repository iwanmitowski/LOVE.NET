namespace LOVE.NET.Services.Dating
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Web.ViewModels.Identity;

    public interface IDatingService
    {
        public IEnumerable<UserMatchViewModel> GetNotSwipedUsers(string userId);

        public UserMatchViewModel GetCurrentMatch(string userId);

        public Task<Result> LikeAsync(string userId, string likedUserId);
    }
}
